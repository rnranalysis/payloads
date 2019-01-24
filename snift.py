import socket
import binascii
import struct

# IPv4 raw socket
def rawSock():
    rawSocket = socket.socket(socket.PF_PACKET, socket.SOCK_RAW, socket.htons(0x0800))
    packet = rawSocket.recvfrom(2048)
    return packet
	
def parseEthernetHeader(packet):
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
def parseIPHeader(packet):
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
    ipLength = packet[0][16:18]
    ipLength = struct.unpack("!2s", ipLength)
    totalipLen = binascii.hexlify(ipLength[0])
    totalipLen = int(totalipLen, 16)
    return protocol, str(totalipLen), srcAddr, dstAddr

def parseTCP(packet, ipLen):
    tcpHeader = packet[0][34:54]
    tcp_hdr = struct.unpack("!2s2s4s4s1s1s2s2s2s", tcpHeader)
    srcPortHex = binascii.hexlify(tcp_hdr[0])
    dstPortHex = binascii.hexlify(tcp_hdr[1])
    seqNum = binascii.hexlify(tcp_hdr[2])
    ackNum = binascii.hexlify(tcp_hdr[3])
    flags = binascii.hexlify(tcp_hdr[5])

def parseUDP(packet, ipLen):
    udpHeader = packet[0][34:42]
    udp_hdr = struct.unpack("!2s2s2s2s", udpHeader)
    srcPort = binascii.hexlify(udp_hdr[0])
    dstPort = binascii.hexlify(udp_hdr[1])
    udpLen = binascii.hexlify(udp_hdr[2])
    udpChkSum = binascii.hexlify(udp_hdr[3])
    udpDataLen = int(udpLen, 16) - 8
    fmt = "!" + str(udpDataLen) + "s"
    s = 42 + udpDataLen
    u = packet[0][42:s]
    d = struct.unpack(fmt, u)
    data = binascii.hexlify(d[0])
    return int(srcPort, 16), int(dstPort, 16), int(str(udpDataLen), 16), udpChkSum, d[0]

def main():
    while True:
        packet = rawSock()# return packet of raw socket
        ethHead = parseEthernetHeader(packet)#returns (destMac, srcMac, ethtype)
        print("Destination Mac: " + ethHead[0])
        print("Source Mac: " + ethHead[1])
        print("Ethernet Type: " + ethHead[2])
        ipHead = parseIPHeader(packet) # returns (protocol, ipLen, srcAddr, destAddr)
        print("Protocol: " + ipHead[0])
        print("Total IP Length: " + ipHead[1])
        ipLen = ipHead[1]
        print("Source Address: " + ipHead[2])
        print("Destination Address: " + ipHead[3])
        if (ipHead[0] == "TCP"):
                tcpHead = parseTCP(packet, ipLen) # returns srcPort, dstPort, seqNum, ackNum, flags
                print("Source Port: " + str(tcpHead[0]))
                print("Destination Port: " + str(tcpHead[1]))
                print("Sequence Number: " + str(tcpHead[2]))
                print("Acknowledgement Number: " + str(tcpHead[3]))
                print("Flags: " + str(tcpHead[4]))
                print("Data: " + str(tcpHead[5]))  
        elif (ipHead[0] == "UDP"):
                udpHead = parseUDP(packet, ipLen) # returns srcPort, dstPort, dataLen, chkSum, data
                print("Source Port: " + str(udpHead[0]))
                print("Destination Port: " + str(udpHead[1]))
                print("Data Length: " + str(udpHead[2]))
                print("Checksum: " + str(udpHead[3]))
                print("Data: " + str(udpHead[4]))

main()
