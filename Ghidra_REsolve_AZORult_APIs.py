# REsolve GetProcAddress -> Global Vars
#@author James Haughom Jr
#@category _NEW_
#@keybinding 
#@menupath 
#@toolbar 

# currently fixing to work across more malware families

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

def main():

	addr = find("GetProcAddress")
	refs = getReferencesTo(addr)
	for ref in refs:
		addrs = re.findall('[a-fA-F0-9]{6,8}', ref.toString())
		getprocaddr = addrs[0]
		if getprocaddr: 
			refs = getReferencesTo(toAddr(getprocaddr))  
			for ref in refs:  
				callee = ref.getFromAddress() 
				inst = getInstructionAt(callee)
				if re.findall('CALL\s0x[a-fA-F0-9]{6,8}', inst.toString()):
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
