# REsolve GetProcAddress -> Global Vars
#@author James Haughom Jr
#@category _NEW_
#@keybinding 
#@menupath 
#@toolbar 


import re

listing = currentProgram.getListing()

def getString(addr):
	string = ''
	byte = getByte(addr)
	while byte != 0x00:
		byte = chr(getByte(addr))
		string += byte
		addr = addr.add(1)
		byte = getByte(addr)
	return(string)

def getSymbol(api, addr):
	createSymbol(toAddr(addr), api, True)


def findGetProcAddress():
	func = getFirstFunction()
	while func is not None:
		if "GetProcAddress" in func.getName():
			print("[+] " + func.getName() + " found at " + (func.getEntryPoint()).toString())
			return func.getEntryPoint()
		else:
			func = getFunctionAfter(func)

def getExtFuncs():
	func_manager = currentProgram.getFunctionManager()
	external_funcs = func_manager.getExternalFunctions()
	while external_funcs.hasNext():
		ext_func = external_funcs.next()
		func_name = ext_func.getName()
		func_loc = ext_func.getExternalLocation()
		func_addr = func_loc.getAddress()
		if "GetProcAddress" in func_name:
			print('[+] ' + func_name + ' located at: ' + func_loc.toString() + ' ' + func_addr.toString())
			refs = getReferencesTo(func_addr)
			for ref in refs:
				callee = ref.getFromAddress()
				if callee:
					inst = getInstructionAt(callee)
					print(inst.toString())
				else:
					print("callee error")

def getFuncs():
	ins_list = listing.getInstructions(1)
	while ins_list.hasNext():
		ins = ins_list.next()
		ops = ins.getOpObjects(0)
		mnemonic = ins.getMnemonicString()
		if mnemonic == "CALL":
			try:
				target_addr = ops[0]
				#func = listing.getFunctionAt(target_addr)
				inst = listing.getInstructionAt(target_addr)
				#print(inst.toString())
				api_addr = re.findall('0x[a-fA-F0-9]{6,8}', inst.toString())
				#print(api_addr[0])
				if api_addr:
					#print(getString(toAddr(api_addr[0])))
					refs = getReferencesTo(toAddr(api_addr[0]))
					for ref in refs:
						callee = ref.getFromAddress()
						inst = getInstructionAt(callee)
						print(inst.toString())
						api_addr = re.findall('0x[a-fA-F0-9]{6,8}', inst.toString())
						print(getString(toAddr(api_addr[0])))
				#func_name = func.getName()
				'''
				if "GetProcAddress" in inst.toString():
					print(inst.toString())
				else:
					print(inst.toString())
					'''
			except:
				continue
		


def main():

	#getprocaddr = findGetProcAddress() 
	getprocaddr = 0x00404e98
	if getprocaddr: 
		refs = getReferencesTo(toAddr(getprocaddr))  
		for ref in refs:  
			callee = ref.getFromAddress() 
			inst = getInstructionAt(callee)
			#print(inst.toString()) 
			if "CALL" in inst.toString(): 
				search_module_name = getInstructionBefore(inst)
				while "PUSH" not in search_module_name.toString(): 
					search_module_name = getInstructionBefore(search_module_name) 
				if "PUSH" in search_module_name.toString(): 
					search_api_name = getInstructionBefore(search_module_name) 
				while "PUSH" not in search_api_name.toString(): 
					search_api_name = getInstructionBefore(search_api_name) 
				if "PUSH" in search_api_name.toString(): 
					api_addr = re.findall('0x[a-fA-F0-9]{6,8}', search_api_name.toString()) 
					if api_addr:
						api = getString(toAddr(api_addr[0])) 
						if ".dll" not in api: 
							print("[+] " + api + " found at: " + api_addr[0])
							inst = re.findall('MOV\s\[0x[a-fA-F0-9]{6,8}\],EAX', getInstructionAfter(inst).toString())
							while not inst:
								inst = re.findall('MOV\s\[0x[a-fA-F0-9]{6,8}\],EAX', getInstructionAfter(inst).toString())
							label_addr = re.findall('0x[a-fA-F0-9]{6,8}', inst[0])
							if inst:
								print("[+] Labeling " + label_addr[0] + " as " + api + "\n")
								getSymbol(api, label_addr[0])
	else:
		print("[-] GetProcAddress not in functions list.")

main()
