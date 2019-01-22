import socket
import binascii
import struct 

### global vars
rawSocket = socket.socket(socket.PF_PACKET, socket.SOCK_RAW, socket.htons(0x0800))
packet = rawSocket.recvfrom(2048)
length = packet[0][16:18]
length = struct.unpack("!2s", length)
totalLen = binascii.hexlify(length[0])
totalLen = int(totalLen, 16)

### parse Ethernet Header
def parseEthernetHeader():
    ethernetHeader = packet[0][0:14]
    eth_hdr = struct.unpack("!6s6s2s", ethernetHeader)
    destMac = binascii.hexlify(eth_hdr[0])
    srcMac = binascii.hexlify(eth_hdr[1])

### get ethernet type/protocol 0x0800 = IPv4, 0x806 = ARP
    protocol = binascii.hexlify(eth_hdr[2])
    if (protocol == "0800"):
        ethtype = "IPv4"
    elif (protocol == "0806"):
        ethtype = "ARP"
    return destMac, srcMac, ethtype

### parse IP header
def parseIPHeader():
    ipHeader = packet[0][14:34]
    protocolPacked = packet[0][23:24]
    prtcl = struct.unpack("!1s", protocolPacked)

### 0x06 = tcp  
    protype = binascii.hexlify(prtcl[0])
    if (protype == "06"):
        protocol = "TCP"
    elif (protype == "11"):
        protocol = "UDP"
    else:
        protocol = protype
    ip_hdr = struct.unpack("!12s4s4s", ipHeader)
    srcAddr = socket.inet_ntoa(ip_hdr[1])
    dstAddr = socket.inet_ntoa(ip_hdr[2])
    length = packet[0][16:18]
    length = struct.unpack("!2s", length)
    return protocol, str(totalLen), srcAddr, dstAddr

def parseData():
### parse data
    dataLen = totalLen - 20 - 14 # 20 bytes for IP Header, 14 bytes for Ethernet Header
    hexDataLen = hex(dataLen)
    data = packet[0][34:totalLen]
    fmt = "!" + str(dataLen) + "s"
    d = struct.unpack(fmt, data)
    return d[0]

def parseTCP():
    tcpHeader = packet[0][34:46]
    tcp_hdr = struct.unpack("!2s2s4s4s", tcpHeader)
    srcPortHex = binascii.hexlify(tcp_hdr[0])
    dstPortHex = binascii.hexlify(tcp_hdr[1])
    return int(srcPortHex, 16), int(dstPortHex, 16)

def main():
    ethHead = parseEthernetHeader() #returns (destMac, srcMac, ethtype)
    print("Destination Mac: " + ethHead[0])
    print("Source Mac: " + ethHead[1])
    print("Ethernet Type: " + ethHead[2])
    ipHead = parseIPHeader() # returns (protocol, total len, srcAddr, destAddr)
    print("Protocol: " + ipHead[0])
    print("Total Length: " + ipHead[1])
    print("Source Address: " + ipHead[2])
    print("Destination Address: " + ipHead[3])
    data = parseData() # returns data
    if (ipHead[0] == "TCP"):
        tcpHead = parseTCP() # returns srcPort, dstPort
        print("Source Port: " + str(tcpHead[0]))
        print("Destination Port: " + str(tcpHead[1]))
    
main()
