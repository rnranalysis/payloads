# REsolve GetProcAddress -> Global Vars
#@author James Haughom Jr, Nikhil Hegde
#@category _NEW_
#@keybinding 
#@menupath 
#@toolbar 

TRUE = 1
FALSE = 0
RENAMESUCCESS = 0
RENAMEERROR = 0

def findRefs(funcAddr):
    """
    Finds references to address funcAddr

    Args:
        funcAddr: address in unicode format
    Raises:
    Returns:
        refList: list of references
    """

    refList = []

    refs = getReferencesTo(toAddr(funcAddr))
    for ref in refs:
        refAddr = ref.fromAddress.toString()
        refList.append(refAddr)

    print ("[+] References found from addresses: " + str(refList))
    return refList

def instMnemonic(inst):
    """
    Determine if instruction is a CALL, PUSH or MOV

    Args:
        inst: instruction of type ghidra.program.database.code.InstructionDB
    Raises:
    Returns:
        instType: string representing type of instruction mnemonic
    """

    if not inst:
        print ("[-]   Instruction not found")
        return None

    instStr = inst.toString()

    if "CALL" in instStr:
        instType = "CALL"
    elif "MOV" in instStr:
        instType = "MOV"
    elif "PUSH" in instStr:
        instType = "PUSH"
    else:
        instType = "OTHER"

    print ("[+]   Instruction type " + instType  + " found")
    return instType

def getArgumentsToCall(callInst):
    """
    Iterates upwards from the CALL instruction until two PUSH or LEA instructions are found.
    A LEA instruction will load the string address into a register and then move it onto the
    stack using a MOV [ESP] instruction.
    If a CALL instruction is found while searching for arguments, the script will exit and raise
    an exception because this is not expected and I don't want to modify the sample unnecessarily.

    Args:
        callInst: CALL instruction of type ghidra.program.database.code.InstructionDB
    Raises:
        None
    Returns:
        argInstList: List of 2 instructions of type ghidra.program.database.code.InstructionDB
                     which indicate the arguments passed to the function.
    """

    argInstList = []
    pushConvention = FALSE

    prevInst = getInstructionBefore(callInst)

    while len(argInstList) < 2:
        prevInstStr = prevInst.toString()
        if "PUSH" in prevInstStr:
            pushConvention = TRUE
            argInstList.append(prevInst)
        elif "LEA" in prevInstStr:
            argInstList.append(prevInst)
        elif "CALL" in prevInstStr:
            raise Exception("CALL found while searching for arguments to CALL instruction " + \
                            "at address 0x" + callInst.getAddress().toString() + ". Exiting!")
        prevInst = getInstructionBefore(prevInst)

    if pushConvention:
        print ("[+]   Arguments to CALL at address 0x" + callInst.getAddress().toString() + \
               " were passed with PUSH convention")
    else:
        print ("[+]   Arguments to CALL at address 0x" + callInst.getAddress().toString() + \
               " were passed with MOV ESP convention")

    print ("[+]   Arguments to CALL at address 0x" + callInst.getAddress().toString() + \
           " are: " + str(argInstList))

    return argInstList

def getWinMethodName(pushInstList):
    """
    Get

    Args:
        pushInstList: List of 2 PUSH instructions of type ghidra.program.database.code.InstructionDB
    Raises:
    Returns:
        lpProcName: Windows method loaded using GetProcAddress in unicode format
    """

    global RENAMEERROR
    winMethodPushInst = pushInstList[1]

    # Find reference from extracted address. It should reference a single address
    # where the Windows method is called from
    lpProcNameRefAddr = winMethodPushInst.getAddress()
    lpProcNameAddr = getReferencesFrom(lpProcNameRefAddr)[0].toAddress.toString()
    print ("[+]   Windows method loaded is located at address: 0x" + lpProcNameAddr)
    
    # Extract Windows method name
    lpProcName = getDataAt(toAddr(lpProcNameAddr))
    if lpProcName:
        lpProcName = lpProcName.toString()
    else:
        print ("[-]   Undefined data encountered at address 0x" + lpProcNameAddr + \
               ". Not modifying anything for this CALL")
        print ("[+]   Possible solution: Go to 0x" + lpProcNameAddr + " and define " + \
               "data as string. Re-run the script.")
        RENAMEERROR = RENAMEERROR + 1
        return None
    lpProcName = lpProcName.split(' ')[-1].replace('"','')
    print ("[+]   Windows method loaded is " + lpProcName)

    return lpProcName

def renameVarAddr(callInst, lpProcName):
    """
    Determines the address of the variable into which address of the Windows method is
    loaded into and renames it appropriately

    Args:
        callInst: CALL instruction of type ghidra.program.database.code.InstructionDB
        lpProcName: Name of Windows method in string format
    Raises:
    Returns:
        TRUE if variable rename is successful
    """

    global RENAMESUCCESS

    nextInst = getInstructionAfter(callInst)

    # Loop until a MOV <variable> EAX instruction is found
    while ("MOV" not in nextInst.toString() or "EAX" not in nextInst.toString()):
        nextInst = getInstructionAfter(nextInst)

    # Get symbol
    varAddr = nextInst.getOpObjects(0)[0]
    varSymbol = getSymbolAt(varAddr)

    # Rename symbol
    try:
        varSymbol.setName("addr_" + lpProcName, ghidra.program.model.symbol.SourceType.USER_DEFINED)
    except:
        print ("[-] Symbol at address 0x" + varAddr.toString() + \
               " could not be modified to addr_" + lpProcName)
        return FALSE

    RENAMESUCCESS = RENAMESUCCESS + 1
    print ("[+]   Symbol at address 0x" + varAddr.toString() + \
           " has been changed to addr_" + lpProcName)

    return TRUE

def renameVariable(callInstAddrList):
    """
    Extract the name of the Windows method that was loaded using GetProcAddress

    Args:
        callInstAddrList: list containing instructions of type 
                  ghidra.program.database.code.InstructionDB
    Raises:
    Returns:
        None
    """

    pushList = []

    for callAddr in callInstAddrList:
        # Get CALL instruction
        callInst = getInstructionAt(toAddr(callAddr))

        # Get arguments to the CALL instruction
        pushInstList = getArgumentsToCall(callInst)

        # Get name of the Windows method that was loaded
        lpProcName = getWinMethodName(pushInstList)

        # Determines the address of the variable into which address of lpProcName
        # is loaded into and renames it
        if (lpProcName and not renameVarAddr(callInst, lpProcName)):
            print ("[-] CALL instruction at address 0x" + callAddr + " needs to be looked at")
    
def getCallInstAddrList(ref, registerName=None, customString=None):
    """
    Builds a list of addresses of instructions in the current function which have the format
    CALL registerName or CALL customString. Provide either registerName or custom
    argument or the program might misbehave. Currently capable of searching only
    CALL registerName instructions

    Args:
        ref: reference address in unicode format
        registerName: name of register in string format
        customString
    Raises:
        None
    Returns:
        callInstAddrList: List containing unicode format addresses of CALL registerName instructions
    """

    callInstAddrList = []
    searchPattern = {"CALL EAX": b"\xFF\xD0", "CALL ECX": b"\xFF\xD1",\
                     "CALL EDX": b"\xFF\xD2", "CALL EBX": b"\xFF\xD3",\
                     "CALL ESP": b"\xFF\xD4", "CALL EBP": b"\xFF\xD5",\
                     "CALL ESI": b"\xFF\xD6", "CALL EDI": b"\xFF\xD7"}

    if registerName and not customString:
        # Get address of last instruction of current function
        parentFuncBody = getFunctionContaining(toAddr(ref)).getBody()
        funcEndAddr = parentFuncBody.getMaxAddress()

        # Search for CALL instructions until the end of the current function
        callAddr = findBytes(toAddr(ref), searchPattern["CALL " + registerName])
        while callAddr and callAddr < funcEndAddr:
            callInstAddrList.append(callAddr.toString())
            ref = getInstructionAfter(callAddr).getAddress().toString()
            callAddr = findBytes(toAddr(ref), searchPattern["CALL " + registerName])
    elif not registerName and customString:
        pass
    elif registerName and customString:
        raise Exception("I think both registerName and customString arguments " + \
                        "were provided. Specify either, but not both.")
    else:
        raise Exception("Something went wrong. Check function arguments")

    print ("[+]   CALL " + registerName + " were found at addresses: " + str(callInstAddrList))

    return callInstAddrList

def getBoundaryInst(ref, registerName, boundary="CALL"):
    """
    Gets the target instruction where the jump to GetProcAddress occurs. Mostly,
    this jump is due to a CALL instruction.

    Args:
        ref: reference address in unicode format
        registerName: string name of register (CALL <reg> format)
        boundary: string name of instruction mnemonic
    Raises:
        None
    Returns:
        inst: instruction of type ghidra.program.database.code.InstructionDB
    """

    inst = getInstructionAfter(toAddr(ref))
    while TRUE:
        if boundary in inst.toString() and registerName in inst.toString():
            return inst
        inst = getInstructionAfter(inst)

def handleMovRef(ref, inst):
    """
    This function handles all steps when the reference address has a
    MOV <reg>, LoadProcAddress instruction format.

    Args:
        ref: reference address in unicode format
        inst: instruction of type ghidra.program.database.code.InstructionDB
    Raises:
        None
    Returns:
        NA
    """

    registerName = inst.getRegister(0).toString()

    # Get the target instruction which jumps to GetProcAddress
    inst = getBoundaryInst(ref, registerName)

    # Build a list of CALL registerName instruction addresses in the current function
    callInstAddrList = getCallInstAddrList(ref, registerName)

    # Rename relevant variables with corresponding Windows methods
    renameVariable(callInstAddrList)

def doMagic(ref, inst, instType):
    """
    Call functions that handle renaming of variables that are associated with
    Windows function that are loaded using GetProcAddress()

    Args:
        ref: reference address in unicode format
        inst: instruction of type ghidra.program.database.code.InstructionDB
        instType: instruction type in string format
    Raises:
        None
    Returns:
        lpProcName: Name of the Windows method that's being loaded using GetProcAddress
    """

    if instType == "MOV":
        handleMovRef(ref, inst)
    elif instType == "CALL":
        #handleCallRef(ref, inst)
        print ("[*] CALL instruction reference. TODO")
        pass
    elif instType == "PUSH":
        #TODO
        print ("[*] PUSH instruction reference. TODO")
        pass
    elif instType == "OTHER":
        print("[-] Unhandleable instruction type found. Continuing...")
        return None
        
def main():
    """
    Args:
        None
    Raises:
        Exception: If GetProcAddress() couldn't be found
    Returns:
        None
    """

    listing = currentProgram.getListing()

    # Get address of GetProcAddress()
    # I'm assuming the first find will be the required address. This is a big assumption
    # and needs to be tested
    getProcAddressAddr = find("GetProcAddress").toString()
    if getProcAddressAddr:
        print("[+] GetProcAddress found at 0x" + getProcAddressAddr)
    else:
        raise Exception("[-] Couldn't find GetProcAddress() call: " + str(e))

    # Get references to GetProcAddress()
    refsList = findRefs(getProcAddressAddr)

    for ref in refsList:
        print ("[+] Analyzing reference address 0x" + ref)
        # Determine instruction type at ref
        inst = getInstructionAt(toAddr(ref))
        instType = instMnemonic(inst)

        # Rename variables that are associated with Windows function that are loaded using
        # GetProcAddress()
        doMagic(ref, inst, instType)

    print ("[+] Total variables successfully renamed: " + str(RENAMESUCCESS))
    print ("[+] Total variables unable to be renamed: " + str(RENAMEERROR))

main()
