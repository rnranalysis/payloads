import socket
import binascii
import struct

# IPv4 raw socket
# hexdump courtesy of https://gist.github.com/sbz/1080258
def hexdump(src, length=16):
    FILTER = ''.join([(len(repr(chr(x))) == 3) and chr(x) or '.' for x in range(256)])
    lines = []
    for c in xrange(0, len(src), length):
        chars = src[c:c+length]
        hex = ' '.join(["%02x" % ord(x) for x in chars])
        printable = ''.join(["%s" % ((ord(x) <= 127 and FILTER[ord(x)]) or '.') for x in chars])
        lines.append("%04x  %-*s  %s\n" % (c, length*3, hex, printable))
    return ''.join(lines)

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
    dataOffset = binascii.hexlify(tcp_hdr[4])
    print("raw: " + str(dataOffset))
    if (dataOffset == 80):
        d = 32
    elif (dataOffset == 50):
        d = 20
    else:
        print("Python sucks")
    winSize = binascii.hexlify(tcp_hdr[6])
    chkSum = binascii.hexlify(tcp_hdr[7])
    urgPtr = binascii.hexlify(tcp_hdr[8])
    return int(srcPortHex, 16), int(dstPortHex, 16), seqNum, ackNum, d, winSize,chkSum, urgPtr

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

def parseDNS(packet, ipLen):
    dnsHeader = packet[0][42:54]
    dns_hdr = struct.unpack("!2s2s2s2s2s2s", dnsHeader)
    transID = binascii.hexlify(dns_hdr[0])
    flags = binascii.hexlify(dns_hdr[1])
    qstCt = binascii.hexlify(dns_hdr[2])
    ansrCt = binascii.hexlify(dns_hdr[3])
    atyCt = binascii.hexlify(dns_hdr[4])
    addCt = binascii.hexlify(dns_hdr[5])
    return transID, flags, str(qstCt), str(ansrCt), str(atyCt), str(addCt) 

def parseHTTP(packet, data):
    d = 14 + int(data, 16)
    r = d + 20
    print("D: " + str(d) + "\nR: " + str(r))
    http = packet[0][d:r]
    print("http: " + http)
    try:
         http_hd = struct.unpack("!20s", http)
         return http_hd[0]
    except:
         print("no data")
         return "no data"

def main():
    pNum = 0
    while True:
        packet = rawSock()# return packet of raw socket
        ethHead = parseEthernetHeader(packet)#returns (destMac, srcMac, ethtype)
        print("\n##############################")
        pNum += pNum + 1
        print("START Packet no. " + str(pNum))
        print("##############################")
        print("-----------Ethernet-----------")
        #print("-----------------------------")
        print("Destination Mac: " + ethHead[0])
        print("Source Mac: " + ethHead[1])
        print("Ethernet Type: " + ethHead[2])
        ipHead = parseIPHeader(packet) # returns (protocol, ipLen, srcAddr, destAddr)
        print("-------------IP--------------")
        print("Protocol: " + ipHead[0])
        print("Total Length: " + ipHead[1])
        ipLen = ipHead[1]
        print("Source Address: " + ipHead[2])
        print("Destination Address: " + ipHead[3])
        if (ipHead[0] == "TCP"):
                tcpHead = parseTCP(packet, ipLen) # returns srcPort, dstPort, seqNum, ackNum, dataOffset, winSize,chkSum, urgPtr
                print("------------TCP------------")
                print("Source Port: " + str(tcpHead[0]))
                print("Destination Port: " + str(tcpHead[1]))
                print("Sequence Number: " + str(tcpHead[2]))
                print("Acknowledgement Number: " + str(tcpHead[3]))
                print("Data Offset: " + str(tcpHead[4]))
                print("Window Size: " + str(tcpHead[5]))
                print("Check sum: " + str(tcpHead[6]))
                print("Urgent Pointer: " + str(tcpHead[7]))
                if (tcpHead[0] == 80 or tcpHead[0] == 443 or tcpHead[1] == 80 or tcpHead[1] == 443):
                     httpHead = parseHTTP(packet, tcpHead[4])
                     print(hexdump(str(httpHead)))
                     
        elif (ipHead[0] == "UDP"):
                udpHead = parseUDP(packet, ipLen) # returns srcPort, dstPort, dataLen, chkSum, data
                print("------------UDP--------------")
                print("Source Port: " + str(udpHead[0]))
                print("Destination Port: " + str(udpHead[1]))
                #print("Data Length: " + str(udpHead[2]))
                print("Checksum: " + str(udpHead[3]))
                if (udpHead[0] == 53 or udpHead[1] == 53):
                    dnsHead = parseDNS(packet, ipLen)
                    print("------------DNS------------")
                    print("Transaction ID: " + dnsHead[0])
                    print("Flags: " + dnsHead[1])
                    print("Question count: " + dnsHead[2])
                    print("Answer RR: " + dnsHead[3])
                    print("Authority RR: " + dnsHead[4])
                    print("Additional RR: " + dnsHead[5])
                else:
                    print("nah")
        print("------------Dump-------------")
        if (ipHead[0] == "UDP"):
             print(hexdump(str(udpHead[4])))
        elif (ipHead[0] == "TCP"):
             print(hexdump(str(httpHead)))
        print("##############################")
        print("END Packet no. " + str(pNum))
        print("##############################\n")
main()
