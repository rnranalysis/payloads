# REsolve GetProcAddress -> Global Vars
#@author James Haughom
#@category _NEW_
#@keybinding 
#@menupath 
#@toolbar 

import re
from java.lang import String

listing = currentProgram.getListing()

def comment(api, addr):
	codeUnit = listing.getCodeUnitAt(addr)
	codeUnit.setComment(codeUnit.EOL_COMMENT, api)

def getSymbol(api, addr):
	createSymbol(toAddr(addr), api, True)

def getString(addr):
	string = ''
	byte = getByte(addr)
	while byte != 0x00:
		byte = chr(getByte(addr))
		string += byte
		addr = addr.add(1)
		byte = getByte(addr)
	return(string)

def main():
	getprocaddr = 0x00404e98 #GetProcAddress

	refs = getReferencesTo(toAddr(getprocaddr))

	for ref in refs:

		callee = ref.getFromAddress()
		inst = getInstructionAt(callee)
		if "CALL" in inst.toString():
			push_eax = getInstructionBefore(inst)
			if "PUSH EAX" in push_eax.toString():
				mov_eax_ebx = getInstructionBefore(push_eax)
				if "MOV EAX" in mov_eax_ebx.toString():
					push_api = getInstructionBefore(mov_eax_ebx)
					push_api = push_api.toString()
					match = re.findall('0x[a-fA-F0-9]{6,8}', push_api)
					if match:
						api = getString(toAddr(match[0]))
						print('[+] API REsolved: ' + api + ' at ' + match[0])
						mov_eax_api = getInstructionAfter(inst)
						if "MOV" and "EAX" in mov_eax_api.toString():
							symbol_addr = re.findall('0x[a-fA-F0-9]{6,8}', mov_eax_api.toString())
							if symbol_addr:
								getSymbol(api, symbol_addr[0])
					else:
						continue


main()
