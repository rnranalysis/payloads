# CURRENTLY WORKING ON ASSIGNING LABELS TO THE GLOBAL VARS


# REsolve GetProcAddress -> Global Vars
#@author James Haughom
#@category _NEW_
#@keybinding 
#@menupath 
#@toolbar 

import re
from java.lang import String

def commentTest():
	listing = currentProgram.getListing()
	codeUnit = listing.getCodeUnitAt(toAddr("0x00415e68"))
	codeUnit.setComment(codeUnit.EOL_COMMENT, "TEST COMMENT!")

def comment(api, addr):
	listing = currentProgram.getListing()
	codeUnit = listing.getCodeUnitAt(addr)
	codeUnit.setComment(codeUnit.EOL_COMMENT, api)


def getSymbol(api, addr):
	createSymbol(toAddr(addr), api, True)


def getString(addr):
	byte = 1
	string = ''
	byte = getByte(addr)
	while byte != 0x00:
		byte = chr(getByte(addr))
		string += byte
		addr = addr.add(1)
		byte = getByte(addr)
	return(string)

def main():
	commentTest()
	listing = currentProgram.getListing()
	loc = 0x00404e98 #GetProcAddress

	refs = getReferencesTo(toAddr(loc))

	for r in refs:

		callee = r.getFromAddress()
		inst = getInstructionAt(callee)
		if "CALL" in inst.toString():
			push_eax = getInstructionBefore(inst)
			if "PUSH EAX" in push_eax.toString():
				mov_eax_ebx = getInstructionBefore(push_eax)
				if "MOV EAX" in mov_eax_ebx.toString():
					#a = re.findall('0x[a-fA-F0-9]{6,8}', mov_eax_ebx.toString())
					push_api = getInstructionBefore(mov_eax_ebx)
					push_api = push_api.toString()
					match = re.findall('0x[a-fA-F0-9]{6,8}', push_api)
					if match:
						api = getString(toAddr(match[0]))
						print('[+] API REsolved: ' + api + ' at ' + match[0])
						#a = re.findall('0x[a-fA-F0-9]{6,8}', mov_eax_ebx.toString())
						#getSymbol(api, a[0])
					else:
						continue


main()


'''
#Imports a file with lines in the form "symbolName 0xADDRESS"
#@category Data
#@author 
 
f = askFile("Give me a file to open", "Go baby go!")

for line in file(f.absolutePath):  # note, cannot use open(), since that is in GhidraScript
    pieces = line.split()
    address = toAddr(long(pieces[1], 16))
    print "creating symbol", pieces[0], "at address", address
    createLabel(address, pieces[0], False)
'''
