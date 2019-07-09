# REsolve GetProcAddress -> Global Vars
#@author James Haughom Jr, Nikhil Hegde
#@category _NEW_
#@keybinding 
#@menupath 
#@toolbar 

TRUE = 1
FALSE = 0

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
    """

    instStr = inst.toString()

    if "CALL" in instStr:
        return "CALL"
    elif "MOV" in instStr:
        return "MOV"
    elif "PUSH" in instStr:
        return "PUSH"
    else:
        return "UNKNOWN"

def getOperand(inst, num):
    """
    Get the specified operand in an instruction

    Args:
        inst: instruction of type ghidra.program.database.code.InstructionDB
        num: integer representing the operand
    Raises:
        None
    Returns:
        operand
    """

    pass

def handleMOVRef(ref, inst):
    """
    This function handles all steps when the reference address has a
    MOV <reg>, LoadProcAddress instruction format.

    Args:
        ref: reference address in unicode format
        inst: instruction of type ghidra.program.database.code.InstructionDB
    Raises:
        None
    Returns:
    """

    instList = []

    registerName = getOperand(inst, num=1)

    inst = getInstructionAfter(toAddr(ref))
    while TRUE:
        if "CALL" in inst.toString():
            break
        instList.append(inst)
        inst = getInstructionAfter(inst)

    #print (instList)

def findLpProcName(ref, inst, instType):
    """
    Determine name of the Windows method that's being loaded using GetProcAddress

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
        handleMOVRef(ref, inst)
        
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
        # Determine instruction type at ref
        inst = getInstructionAt(toAddr(ref))
        instType = instMnemonic(inst)
        print ("[+] Instruction type " + instType + " found at address: 0x" + ref)

        findLpProcName(ref, inst, instType)

main()
