using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace DataParser
{
    public ref struct DataPacket
    {
        public string data;
        public string protocol;
        public string packetLength;
        public string packetID;
        public string packetType;
        public string avlPacketID;
        public string imeiLength;
        public string moduleImei;
        public string preamble;
        public string dataFieldLength;
        public string codecID;
        public string numberOfData1;
        public string numberOfData2;
        public string crc16;
        public string timestamp;
        public string priority;
        public string longitude;
        public string latitude;
        public string altitude;
        public string angle;
        public string satellites;
        public string speed;
        public string eventID;
        public string numberOfElements;
        public Span<string> xbElementCount;
        public Span<string> elementID;
        public Span<string> elementLength;
        public Span<string> elementValue;
    }

    class Program
    {
        static void InputHandler()
        {
            Console.WriteLine("Write data or command: ");
            string inputedDataStr = Console.ReadLine();

            if (inputedDataStr.StartsWith("."))
            {
                CommandHandler(inputedDataStr);
            }
            else
            {
                DataHandler(inputedDataStr);
            }

            InputHandler();
        }

        static void CommandHandler(string command)
        {
            switch (command)
            {
                case ".exit":
                    Environment.Exit(0);
                    break;
                case ".free":
                    Console.WriteLine("Unrealized command.");
                    break;
                case ".show":
                    Console.WriteLine("Unrealized command.");
                    break;
                case ".info":
                    Console.WriteLine("Unrealized command.");
                    break;
                default:
                    Console.WriteLine("Unexpected command.");
                    break;
            }
        }

        static void DataHandler(string data)
        {
            DataPacket hexTemp = new DataPacket();
            DataPacket valueTemp = new DataPacket();
            DataPacket sizeTemp = new DataPacket();

            if (data[..8] == "00000000") // IF TCP
            {
                if (data[16..18] == "08") // IF Codec 8 over TCP
                {
                    hexTemp.data = data;
                    hexTemp.protocol = data[..8];

                    hexTemp.preamble = data[..8];
                    valueTemp.preamble = GetValue(hexTemp.preamble);
                    sizeTemp.preamble = GetSize(hexTemp.preamble);

                    hexTemp.dataFieldLength = data[8..16];
                    valueTemp.dataFieldLength = GetValue(hexTemp.dataFieldLength);
                    sizeTemp.dataFieldLength = GetSize(hexTemp.dataFieldLength);

                    hexTemp.codecID = data[16..18];
                    valueTemp.codecID = GetValue(hexTemp.codecID);
                    sizeTemp.codecID = GetSize(hexTemp.codecID);

                    hexTemp.numberOfData1 = data[18..20];
                    valueTemp.numberOfData1 = GetValue(hexTemp.numberOfData1);
                    sizeTemp.numberOfData1 = GetSize(hexTemp.numberOfData1);

                    int dfl = GetValueNumeric(hexTemp.dataFieldLength) * 2;
                    hexTemp.numberOfData2 = data[(14 + dfl)..(16 + dfl)];
                    valueTemp.numberOfData2 = GetValue(hexTemp.numberOfData2);
                    sizeTemp.numberOfData2 = GetSize(hexTemp.numberOfData2);

                    if (hexTemp.numberOfData1 != hexTemp.numberOfData2)
                    {
                        Console.WriteLine("Corrupted data inserted.");
                    }

                    hexTemp.crc16 = data[(16 + dfl)..(24 + dfl)];
                    valueTemp.crc16 = GetValue(hexTemp.crc16);
                    sizeTemp.crc16 = GetSize(hexTemp.crc16);

                    hexTemp.timestamp = data[20..36];
                    valueTemp.timestamp = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(long.Parse(hexTemp.timestamp, System.Globalization.NumberStyles.HexNumber)).ToString();
                    sizeTemp.timestamp = GetSize(hexTemp.timestamp);

                    hexTemp.priority = data[36..38];
                    valueTemp.priority = GetValue(hexTemp.priority);
                    sizeTemp.priority = GetSize(hexTemp.priority);

                    hexTemp.longitude = data[38..46];
                    if (Convert.ToString(Convert.ToInt64(hexTemp.longitude, 16), 2).PadLeft(32, '0').Substring(0, 1) == "0")
                    {
                        valueTemp.longitude = GetValue(hexTemp.longitude);
                    }
                    else
                    {
                        valueTemp.longitude = (long.Parse(hexTemp.longitude, System.Globalization.NumberStyles.HexNumber) * -1).ToString();
                    }
                    sizeTemp.longitude = GetSize(hexTemp.longitude);

                    hexTemp.latitude = data[46..54];
                    if (Convert.ToString(Convert.ToInt64(hexTemp.latitude, 16), 2).PadLeft(32, '0').Substring(0, 1) == "0")
                    {
                        valueTemp.latitude = GetValue(hexTemp.latitude);
                    }
                    else
                    {
                        valueTemp.latitude = (long.Parse(hexTemp.latitude, System.Globalization.NumberStyles.HexNumber) * -1).ToString();
                    }
                    sizeTemp.latitude = GetSize(hexTemp.latitude);

                    hexTemp.altitude = data[54..58];
                    if (Convert.ToString(Convert.ToInt64(hexTemp.altitude, 16), 2).PadLeft(16, '0').Substring(0, 1) == "0")
                    {
                        valueTemp.altitude = GetValue(hexTemp.altitude);
                    }
                    else
                    {
                        valueTemp.altitude = (long.Parse(hexTemp.altitude, System.Globalization.NumberStyles.HexNumber) * -1).ToString();
                    }
                    sizeTemp.altitude = GetSize(hexTemp.altitude);

                    hexTemp.angle = data[58..62];
                    valueTemp.angle = GetValue(hexTemp.angle);
                    sizeTemp.angle = GetSize(hexTemp.angle);

                    hexTemp.satellites = data[62..64];
                    valueTemp.satellites = GetValue(hexTemp.satellites);
                    sizeTemp.satellites = GetSize(hexTemp.satellites);

                    hexTemp.speed = data[64..68];
                    valueTemp.speed = GetValue(hexTemp.speed);
                    sizeTemp.speed = GetSize(hexTemp.speed);

                    hexTemp.eventID = data[68..70];
                    valueTemp.eventID = GetValue(hexTemp.eventID);
                    sizeTemp.eventID = GetSize(hexTemp.eventID);

                    hexTemp.numberOfElements = data[70..72];
                    valueTemp.numberOfElements = GetValue(hexTemp.numberOfElements);
                    sizeTemp.numberOfElements = GetSize(hexTemp.numberOfElements);

                    int pos = 72;
                    string[] xbElementCount = new string[4];
                    List<String> elementID = new List<String>();
                    List<String> elementValue = new List<String>();
                    for (int i = 0; i < 4; i++)
                    {
                        xbElementCount[i] = data[pos..(pos += 2)];

                        for (int e = 0; e < GetValueNumeric(xbElementCount[i]); e++)
                        {
                            elementID.Add(data[pos..(pos += 2)]);
                            elementValue.Add(data[pos..(pos += Convert.ToInt32(Math.Pow(2, i + 1)))]);
                        }
                    }

                    hexTemp.xbElementCount = xbElementCount;
                    string[] xbElementCountV = new string[4];
                    string[] xbElementCountS = new string[4];
                    for (int i = 0; i < 4; i++)
                    {
                        xbElementCountV[i] = (GetValue(hexTemp.xbElementCount[i]));
                        xbElementCountS[i] = (GetSize(hexTemp.xbElementCount[i]));
                    }
                    valueTemp.xbElementCount = xbElementCountV;
                    sizeTemp.xbElementCount = xbElementCountS;

                    hexTemp.elementID = elementID.ToArray();
                    hexTemp.elementValue = elementValue.ToArray();

                    elementID.Clear();
                    elementValue.Clear();
                    for (int i = 0; i < GetValueNumeric(hexTemp.numberOfElements); i++)
                    {
                        elementID.Add(GetValue(hexTemp.elementID[i]));
                        elementValue.Add(GetValue(hexTemp.elementValue[i]));
                    }
                    valueTemp.elementID = elementID.ToArray();
                    valueTemp.elementValue = elementValue.ToArray();

                    elementID.Clear();
                    elementValue.Clear();
                    for (int i = 0; i < GetValueNumeric(hexTemp.numberOfElements); i++)
                    {
                        elementID.Add(GetSize(hexTemp.elementID[i]));
                        elementValue.Add(GetSize(hexTemp.elementValue[i]));
                    }
                    sizeTemp.elementID = elementID.ToArray();
                    sizeTemp.elementValue = elementValue.ToArray();


                    PrintModule(hexTemp, valueTemp, sizeTemp);
                }
                else if (data[16..18] == "8E") // IF Codec 8 Extended over TCP
                {
                    hexTemp.data = data;
                    hexTemp.protocol = data[..8];

                    hexTemp.preamble = data[..8];
                    valueTemp.preamble = GetValue(hexTemp.preamble);
                    sizeTemp.preamble = GetSize(hexTemp.preamble);

                    hexTemp.dataFieldLength = data[8..16];
                    valueTemp.dataFieldLength = GetValue(hexTemp.dataFieldLength);
                    sizeTemp.dataFieldLength = GetSize(hexTemp.dataFieldLength);

                    hexTemp.codecID = data[16..18];
                    valueTemp.codecID = GetValue(hexTemp.codecID);
                    sizeTemp.codecID = GetSize(hexTemp.codecID);

                    hexTemp.numberOfData1 = data[18..20];
                    valueTemp.numberOfData1 = GetValue(hexTemp.numberOfData1);
                    sizeTemp.numberOfData1 = GetSize(hexTemp.numberOfData1);

                    int dfl = GetValueNumeric(hexTemp.dataFieldLength) * 2;
                    hexTemp.numberOfData2 = data[(14 + dfl)..(16 + dfl)];
                    valueTemp.numberOfData2 = GetValue(hexTemp.numberOfData2);
                    sizeTemp.numberOfData2 = GetSize(hexTemp.numberOfData2);

                    if (hexTemp.numberOfData1 != hexTemp.numberOfData2)
                    {
                        Console.WriteLine("Corrupted data inserted.");
                    }

                    hexTemp.crc16 = data[(16 + dfl)..(24 + dfl)];
                    valueTemp.crc16 = GetValue(hexTemp.crc16);
                    sizeTemp.crc16 = GetSize(hexTemp.crc16);

                    hexTemp.timestamp = data[20..36];
                    valueTemp.timestamp = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(long.Parse(hexTemp.timestamp, System.Globalization.NumberStyles.HexNumber)).ToString();
                    sizeTemp.timestamp = GetSize(hexTemp.timestamp);

                    hexTemp.priority = data[36..38];
                    valueTemp.priority = GetValue(hexTemp.priority);
                    sizeTemp.priority = GetSize(hexTemp.priority);

                    hexTemp.longitude = data[38..46];
                    if (Convert.ToString(Convert.ToInt64(hexTemp.longitude, 16), 2).PadLeft(32, '0').Substring(0, 1) == "0")
                    {
                        valueTemp.longitude = GetValue(hexTemp.longitude);
                    }
                    else
                    {
                        valueTemp.longitude = (long.Parse(hexTemp.longitude, System.Globalization.NumberStyles.HexNumber) * -1).ToString();
                    }
                    sizeTemp.longitude = GetSize(hexTemp.longitude);

                    hexTemp.latitude = data[46..54];
                    if (Convert.ToString(Convert.ToInt64(hexTemp.latitude, 16), 2).PadLeft(32, '0').Substring(0, 1) == "0")
                    {
                        valueTemp.latitude = GetValue(hexTemp.latitude);
                    }
                    else
                    {
                        valueTemp.latitude = (long.Parse(hexTemp.latitude, System.Globalization.NumberStyles.HexNumber) * -1).ToString();
                    }
                    sizeTemp.latitude = GetSize(hexTemp.latitude);

                    hexTemp.altitude = data[54..58];
                    if (Convert.ToString(Convert.ToInt64(hexTemp.altitude, 16), 2).PadLeft(16, '0').Substring(0, 1) == "0")
                    {
                        valueTemp.altitude = GetValue(hexTemp.altitude);
                    }
                    else
                    {
                        valueTemp.altitude = (long.Parse(hexTemp.altitude, System.Globalization.NumberStyles.HexNumber) * -1).ToString();
                    }
                    sizeTemp.altitude = GetSize(hexTemp.altitude);

                    hexTemp.angle = data[58..62];
                    valueTemp.angle = GetValue(hexTemp.angle);
                    sizeTemp.angle = GetSize(hexTemp.angle);

                    hexTemp.satellites = data[62..64];
                    valueTemp.satellites = GetValue(hexTemp.satellites);
                    sizeTemp.satellites = GetSize(hexTemp.satellites);

                    hexTemp.speed = data[64..68];
                    valueTemp.speed = GetValue(hexTemp.speed);
                    sizeTemp.speed = GetSize(hexTemp.speed);

                    hexTemp.eventID = data[68..72];
                    valueTemp.eventID = GetValue(hexTemp.eventID);
                    sizeTemp.eventID = GetSize(hexTemp.eventID);

                    hexTemp.numberOfElements = data[72..76];
                    valueTemp.numberOfElements = GetValue(hexTemp.numberOfElements);
                    sizeTemp.numberOfElements = GetSize(hexTemp.numberOfElements);

                    int pos = 76;
                    string[] xbElementCount = new string[5];
                    List<String> elementID = new List<String>();
                    List<String> elementLength = new List<String>();
                    List<String> elementValue = new List<String>();
                    for (int i = 0; i < 5; i++)
                    {
                        xbElementCount[i] = data[pos..(pos += 4)];

                        if (i != 4)
                        {
                            for (int e = 0; e < GetValueNumeric(xbElementCount[i]); e++)
                            {
                                elementID.Add(data[pos..(pos += 4)]);
                                elementValue.Add(data[pos..(pos += Convert.ToInt32(Math.Pow(2, i + 1)))]);
                            }
                        }
                        else
                        {
                            for (int e = 0; e < GetValueNumeric(xbElementCount[i]); e++)
                            {
                                elementID.Add(data[pos..(pos += 4)]);
                                elementLength.Add(data[pos..(pos += 4)]);
                                elementValue.Add(data[pos..(pos += GetValueNumeric(elementLength.Last()) * 2)]);
                            }
                        }
                    }

                    hexTemp.xbElementCount = xbElementCount;
                    string[] xbElementCountV = new string[5];
                    string[] xbElementCountS = new string[5];
                    for (int i = 0; i < 5; i++)
                    {
                        xbElementCountV[i] = (GetValue(hexTemp.xbElementCount[i]));
                        xbElementCountS[i] = (GetSize(hexTemp.xbElementCount[i]));
                    }
                    valueTemp.xbElementCount = xbElementCountV;
                    sizeTemp.xbElementCount = xbElementCountS;

                    hexTemp.elementID = elementID.ToArray();
                    hexTemp.elementLength = elementLength.ToArray();
                    hexTemp.elementValue = elementValue.ToArray();

                    elementID.Clear();
                    elementValue.Clear();
                    for (int i = 0; i < GetValueNumeric(hexTemp.numberOfElements); i++)
                    {
                        elementID.Add(GetValue(hexTemp.elementID[i]));
                        elementValue.Add(GetValue(hexTemp.elementValue[i]));
                    }
                    valueTemp.elementID = elementID.ToArray();
                    valueTemp.elementValue = elementValue.ToArray();

                    elementID.Clear();
                    elementValue.Clear();
                    for (int i = 0; i < GetValueNumeric(hexTemp.numberOfElements); i++)
                    {
                        elementID.Add(GetSize(hexTemp.elementID[i]));
                        elementValue.Add(GetSize(hexTemp.elementValue[i]));
                    }
                    sizeTemp.elementID = elementID.ToArray();
                    sizeTemp.elementValue = elementValue.ToArray();


                    PrintModule(hexTemp, valueTemp, sizeTemp);
                }
                else
                {
                    Console.WriteLine("Corrupted data inserted.");
                }

            }
            else if (data[8..10] == "01") // IF UDP
            {
                if (data[46..48] == "08") // IF Codec 8 over UDP
                {
                    hexTemp.data = data;
                    hexTemp.protocol = data[8..10];

                    hexTemp.packetLength = data[..4];
                    valueTemp.packetLength = GetValue(hexTemp.packetLength);
                    sizeTemp.packetLength = GetSize(hexTemp.packetLength);

                    hexTemp.packetID = data[4..8];
                    valueTemp.packetID = GetValue(hexTemp.packetID);
                    sizeTemp.packetID = GetSize(hexTemp.packetID);

                    hexTemp.packetType = data[8..10];
                    valueTemp.packetType = GetValue(hexTemp.packetType);
                    sizeTemp.packetType = GetSize(hexTemp.packetType);

                    hexTemp.avlPacketID = data[10..12];
                    valueTemp.avlPacketID = GetValue(hexTemp.avlPacketID);
                    sizeTemp.avlPacketID = GetSize(hexTemp.avlPacketID);

                    hexTemp.imeiLength = data[12..16];
                    valueTemp.imeiLength = GetValue(hexTemp.imeiLength);
                    sizeTemp.imeiLength = GetSize(hexTemp.imeiLength);

                    hexTemp.moduleImei = data[16..46];
                    valueTemp.moduleImei = new string (hexTemp.moduleImei.Where((ch, index) => index % 2 != 0).ToArray());
                    sizeTemp.moduleImei = GetSize(hexTemp.moduleImei);

                    hexTemp.codecID = data[46..48];
                    valueTemp.codecID = GetValue(hexTemp.codecID);
                    sizeTemp.codecID = GetSize(hexTemp.codecID);

                    hexTemp.numberOfData1 = data[48..50];
                    valueTemp.numberOfData1 = GetValue(hexTemp.numberOfData1);
                    sizeTemp.numberOfData1 = GetSize(hexTemp.numberOfData1);

                    hexTemp.numberOfData2 = data.Substring(data.Length - 2);
                    valueTemp.numberOfData2 = GetValue(hexTemp.numberOfData2);
                    sizeTemp.numberOfData2 = GetSize(hexTemp.numberOfData2);

                    if (hexTemp.numberOfData1 != hexTemp.numberOfData2)
                    {
                        Console.WriteLine("Corrupted data inserted.");
                    }

                    hexTemp.timestamp = data[50..66];
                    valueTemp.timestamp = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(long.Parse(hexTemp.timestamp, System.Globalization.NumberStyles.HexNumber)).ToString();
                    sizeTemp.timestamp = GetSize(hexTemp.timestamp);

                    hexTemp.priority = data[66..68];
                    valueTemp.priority = GetValue(hexTemp.priority);
                    sizeTemp.priority = GetSize(hexTemp.priority);

                    hexTemp.longitude = data[68..76];
                    if (Convert.ToString(Convert.ToInt64(hexTemp.longitude, 16), 2).PadLeft(32, '0').Substring(0, 1) == "0")
                    {
                        valueTemp.longitude = GetValue(hexTemp.longitude);
                    }
                    else
                    {
                        valueTemp.longitude = (long.Parse(hexTemp.longitude, System.Globalization.NumberStyles.HexNumber) * -1).ToString();
                    }
                    sizeTemp.longitude = GetSize(hexTemp.longitude);

                    hexTemp.latitude = data[76..84];
                    if (Convert.ToString(Convert.ToInt64(hexTemp.latitude, 16), 2).PadLeft(32, '0').Substring(0, 1) == "0")
                    {
                        valueTemp.latitude = GetValue(hexTemp.latitude);
                    }
                    else
                    {
                        valueTemp.latitude = (long.Parse(hexTemp.latitude, System.Globalization.NumberStyles.HexNumber) * -1).ToString();
                    }
                    sizeTemp.latitude = GetSize(hexTemp.latitude);

                    hexTemp.altitude = data[84..88];
                    if (Convert.ToString(Convert.ToInt64(hexTemp.altitude, 16), 2).PadLeft(16, '0').Substring(0, 1) == "0")
                    {
                        valueTemp.altitude = GetValue(hexTemp.altitude);
                    }
                    else
                    {
                        valueTemp.altitude = (long.Parse(hexTemp.altitude, System.Globalization.NumberStyles.HexNumber) * -1).ToString();
                    }
                    sizeTemp.altitude = GetSize(hexTemp.altitude);

                    hexTemp.angle = data[88..92];
                    valueTemp.angle = GetValue(hexTemp.angle);
                    sizeTemp.angle = GetSize(hexTemp.angle);

                    hexTemp.satellites = data[92..94];
                    valueTemp.satellites = GetValue(hexTemp.satellites);
                    sizeTemp.satellites = GetSize(hexTemp.satellites);

                    hexTemp.speed = data[94..98];
                    valueTemp.speed = GetValue(hexTemp.speed);
                    sizeTemp.speed = GetSize(hexTemp.speed);

                    hexTemp.eventID = data[98..100];
                    valueTemp.eventID = GetValue(hexTemp.eventID);
                    sizeTemp.eventID = GetSize(hexTemp.eventID);

                    hexTemp.numberOfElements = data[100..102];
                    valueTemp.numberOfElements = GetValue(hexTemp.numberOfElements);
                    sizeTemp.numberOfElements = GetSize(hexTemp.numberOfElements);

                    int pos = 102;
                    string[] xbElementCount = new string[4];
                    List<String> elementID = new List<String>();
                    List<String> elementValue = new List<String>();
                    for (int i = 0; i < 4; i++)
                    {
                        xbElementCount[i] = data[pos..(pos += 2)];

                        for (int e = 0; e < GetValueNumeric(xbElementCount[i]); e++)
                        {
                            elementID.Add(data[pos..(pos += 2)]);
                            elementValue.Add(data[pos..(pos += Convert.ToInt32(Math.Pow(2, i + 1)))]);
                        }
                    }

                    hexTemp.xbElementCount = xbElementCount;
                    string[] xbElementCountV = new string[4];
                    string[] xbElementCountS = new string[4];
                    for (int i = 0; i < 4; i++)
                    {
                        xbElementCountV[i] = (GetValue(hexTemp.xbElementCount[i]));
                        xbElementCountS[i] = (GetSize(hexTemp.xbElementCount[i]));
                    }
                    valueTemp.xbElementCount = xbElementCountV;
                    sizeTemp.xbElementCount = xbElementCountS;

                    hexTemp.elementID = elementID.ToArray();
                    hexTemp.elementValue = elementValue.ToArray();

                    elementID.Clear();
                    elementValue.Clear();
                    for (int i = 0; i < GetValueNumeric(hexTemp.numberOfElements); i++)
                    {
                        elementID.Add(GetValue(hexTemp.elementID[i]));
                        elementValue.Add(GetValue(hexTemp.elementValue[i]));
                    }
                    valueTemp.elementID = elementID.ToArray();
                    valueTemp.elementValue = elementValue.ToArray();

                    elementID.Clear();
                    elementValue.Clear();
                    for (int i = 0; i < GetValueNumeric(hexTemp.numberOfElements); i++)
                    {
                        elementID.Add(GetSize(hexTemp.elementID[i]));
                        elementValue.Add(GetSize(hexTemp.elementValue[i]));
                    }
                    sizeTemp.elementID = elementID.ToArray();
                    sizeTemp.elementValue = elementValue.ToArray();


                    PrintModule(hexTemp, valueTemp, sizeTemp);
                }
                else if (data[46..48] == "8E") // IF Codec 8 Extended over UDP
                {
                    hexTemp.data = data;
                    hexTemp.protocol = data[8..10];

                    hexTemp.packetLength = data[..4];
                    valueTemp.packetLength = GetValue(hexTemp.packetLength);
                    sizeTemp.packetLength = GetSize(hexTemp.packetLength);

                    hexTemp.packetID = data[4..8];
                    valueTemp.packetID = GetValue(hexTemp.packetID);
                    sizeTemp.packetID = GetSize(hexTemp.packetID);

                    hexTemp.packetType = data[8..10];
                    valueTemp.packetType = GetValue(hexTemp.packetType);
                    sizeTemp.packetType = GetSize(hexTemp.packetType);

                    hexTemp.avlPacketID = data[10..12];
                    valueTemp.avlPacketID = GetValue(hexTemp.avlPacketID);
                    sizeTemp.avlPacketID = GetSize(hexTemp.avlPacketID);

                    hexTemp.imeiLength = data[12..16];
                    valueTemp.imeiLength = GetValue(hexTemp.imeiLength);
                    sizeTemp.imeiLength = GetSize(hexTemp.imeiLength);

                    hexTemp.moduleImei = data[16..46];
                    valueTemp.moduleImei = new string(hexTemp.moduleImei.Where((ch, index) => index % 2 != 0).ToArray());
                    sizeTemp.moduleImei = GetSize(hexTemp.moduleImei);

                    hexTemp.codecID = data[46..48];
                    valueTemp.codecID = GetValue(hexTemp.codecID);
                    sizeTemp.codecID = GetSize(hexTemp.codecID);

                    hexTemp.numberOfData1 = data[48..50];
                    valueTemp.numberOfData1 = GetValue(hexTemp.numberOfData1);
                    sizeTemp.numberOfData1 = GetSize(hexTemp.numberOfData1);

                    hexTemp.numberOfData2 = data.Substring(data.Length - 2);
                    valueTemp.numberOfData2 = GetValue(hexTemp.numberOfData2);
                    sizeTemp.numberOfData2 = GetSize(hexTemp.numberOfData2);

                    if (hexTemp.numberOfData1 != hexTemp.numberOfData2)
                    {
                        Console.WriteLine("Corrupted data inserted.");
                    }

                    hexTemp.timestamp = data[50..66];
                    valueTemp.timestamp = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(long.Parse(hexTemp.timestamp, System.Globalization.NumberStyles.HexNumber)).ToString();
                    sizeTemp.timestamp = GetSize(hexTemp.timestamp);

                    hexTemp.priority = data[66..68];
                    valueTemp.priority = GetValue(hexTemp.priority);
                    sizeTemp.priority = GetSize(hexTemp.priority);

                    hexTemp.longitude = data[68..76];
                    if (Convert.ToString(Convert.ToInt64(hexTemp.longitude, 16), 2).PadLeft(32, '0').Substring(0, 1) == "0")
                    {
                        valueTemp.longitude = GetValue(hexTemp.longitude);
                    }
                    else
                    {
                        valueTemp.longitude = (long.Parse(hexTemp.longitude, System.Globalization.NumberStyles.HexNumber) * -1).ToString();
                    }
                    sizeTemp.longitude = GetSize(hexTemp.longitude);

                    hexTemp.latitude = data[76..84];
                    if (Convert.ToString(Convert.ToInt64(hexTemp.latitude, 16), 2).PadLeft(32, '0').Substring(0, 1) == "0")
                    {
                        valueTemp.latitude = GetValue(hexTemp.latitude);
                    }
                    else
                    {
                        valueTemp.latitude = (long.Parse(hexTemp.latitude, System.Globalization.NumberStyles.HexNumber) * -1).ToString();
                    }
                    sizeTemp.latitude = GetSize(hexTemp.latitude);

                    hexTemp.altitude = data[84..88];
                    if (Convert.ToString(Convert.ToInt64(hexTemp.altitude, 16), 2).PadLeft(16, '0').Substring(0, 1) == "0")
                    {
                        valueTemp.altitude = GetValue(hexTemp.altitude);
                    }
                    else
                    {
                        valueTemp.altitude = (long.Parse(hexTemp.altitude, System.Globalization.NumberStyles.HexNumber) * -1).ToString();
                    }
                    sizeTemp.altitude = GetSize(hexTemp.altitude);

                    hexTemp.angle = data[88..92];
                    valueTemp.angle = GetValue(hexTemp.angle);
                    sizeTemp.angle = GetSize(hexTemp.angle);

                    hexTemp.satellites = data[92..94];
                    valueTemp.satellites = GetValue(hexTemp.satellites);
                    sizeTemp.satellites = GetSize(hexTemp.satellites);

                    hexTemp.speed = data[94..98];
                    valueTemp.speed = GetValue(hexTemp.speed);
                    sizeTemp.speed = GetSize(hexTemp.speed);

                    hexTemp.eventID = data[98..102];
                    valueTemp.eventID = GetValue(hexTemp.eventID);
                    sizeTemp.eventID = GetSize(hexTemp.eventID);

                    hexTemp.numberOfElements = data[102..106];
                    valueTemp.numberOfElements = GetValue(hexTemp.numberOfElements);
                    sizeTemp.numberOfElements = GetSize(hexTemp.numberOfElements);

                    int pos = 106;
                    string[] xbElementCount = new string[5];
                    List<String> elementID = new List<String>();
                    List<String> elementLength = new List<String>();
                    List<String> elementValue = new List<String>();
                    for (int i = 0; i < 5; i++)
                    {
                        xbElementCount[i] = data[pos..(pos += 4)];
                        Console.WriteLine("xb count: " + xbElementCount[i]);

                        if (i != 4)
                        {
                            for (int e = 0; e < GetValueNumeric(xbElementCount[i]); e++)
                            {
                                elementID.Add(data[pos..(pos += 4)]);
                                elementValue.Add(data[pos..(pos += Convert.ToInt32(Math.Pow(2, i + 1)))]);
                            }
                        }
                        else
                        {
                            for (int e = 0; e < GetValueNumeric(xbElementCount[i]); e++)
                            {
                                elementID.Add(data[pos..(pos += 4)]);
                                elementLength.Add(data[pos..(pos += 4)]);
                                elementValue.Add(data[pos..(pos += GetValueNumeric(elementLength.Last()) * 2)]);
                            }
                        }
                    }

                    hexTemp.xbElementCount = xbElementCount;
                    string[] xbElementCountV = new string[5];
                    string[] xbElementCountS = new string[5];
                    for (int i = 0; i < 5; i++)
                    {
                        xbElementCountV[i] = (GetValue(hexTemp.xbElementCount[i]));
                        xbElementCountS[i] = (GetSize(hexTemp.xbElementCount[i]));
                    }
                    valueTemp.xbElementCount = xbElementCountV;
                    sizeTemp.xbElementCount = xbElementCountS;

                    hexTemp.elementID = elementID.ToArray();
                    hexTemp.elementLength = elementLength.ToArray();
                    hexTemp.elementValue = elementValue.ToArray();

                    elementID.Clear();
                    elementValue.Clear();
                    for (int i = 0; i < GetValueNumeric(hexTemp.numberOfElements); i++)
                    {
                        elementID.Add(GetValue(hexTemp.elementID[i]));
                        elementValue.Add(GetValue(hexTemp.elementValue[i]));
                    }
                    valueTemp.elementID = elementID.ToArray();
                    valueTemp.elementValue = elementValue.ToArray();

                    elementID.Clear();
                    elementValue.Clear();
                    for (int i = 0; i < GetValueNumeric(hexTemp.numberOfElements); i++)
                    {
                        elementID.Add(GetSize(hexTemp.elementID[i]));
                        elementValue.Add(GetSize(hexTemp.elementValue[i]));
                    }
                    sizeTemp.elementID = elementID.ToArray();
                    sizeTemp.elementValue = elementValue.ToArray();


                    PrintModule(hexTemp, valueTemp, sizeTemp);
                }
                else
                {
                    Console.WriteLine("Corrupted data inserted.");
                }
            }
            else
            {
                Console.WriteLine("Corrupted data inserted.");
            }
        }

        static string GetValue(string x)
        {
            return ulong.Parse(x, System.Globalization.NumberStyles.HexNumber).ToString();
        }

        static int GetValueNumeric(string x)
        {
            return int.Parse(x, System.Globalization.NumberStyles.HexNumber);
        }

        static string GetSize(string x)
        {
            return (x.Length / 2).ToString();
        }

        static void DataStorage(string protocol, string codec, string data)
        {

        }

        static void PrintModule(DataPacket hex, DataPacket value, DataPacket size)
        {
            int cw1 = -40, cw2 = -15, cw3 = -30, cw4 = -30;
            if (hex.protocol == "00000000") // IF TCP
            {
                Console.WriteLine("-------------------------------------------------------------------------------------------------------------");
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "Name", "Size", "Value", "Hex Value"));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}", "TCP AVL Data Packet", "var"));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "  Preamble", size.preamble, value.preamble, hexSplit(hex.preamble)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "  AVL Data Length", size.dataFieldLength, value.dataFieldLength, hexSplit(hex.dataFieldLength)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}", "  Data", "var"));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "    Codec ID", size.codecID, value.codecID, hexSplit(hex.codecID)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "    AVL Data Count", size.numberOfData1, value.numberOfData1, hexSplit(hex.numberOfData1)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}", "    AVL Data", "var"));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "      Timestamp", size.timestamp, value.timestamp, hexSplit(hex.timestamp)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "      Priority", size.priority, value.priority, hexSplit(hex.priority)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}", "      GPS Element", "15"));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        Longitude", size.longitude, value.longitude, hexSplit(hex.longitude)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        Latitude", size.latitude, value.latitude, hexSplit(hex.latitude)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        Altitude", size.altitude, value.altitude, hexSplit(hex.altitude)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        Angle", size.angle, value.angle, hexSplit(hex.angle)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        Satellites", size.satellites, value.satellites, hexSplit(hex.satellites)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        Speed", size.speed, value.speed, hexSplit(hex.speed)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}", "      I/O Element", "var"));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        Event ID", size.eventID, value.eventID, hexSplit(hex.eventID)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        Element count", size.numberOfElements, value.numberOfElements, hexSplit(hex.numberOfElements)));
                int pos = 0;
                if (hex.codecID == "08") // IF Codec 8 over TCP
                {
                    for (int i = 0; i < hex.xbElementCount.Length; i++)
                    {
                        Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        " + (Math.Pow(2, i)) + "b Element count", size.xbElementCount[i], value.xbElementCount[i], hexSplit(hex.xbElementCount[i])));

                        for (int e = pos; e < pos + GetValueNumeric(hex.xbElementCount[i]); e++)
                        {
                            Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        ID", size.elementID[e], value.elementID[e], hexSplit(hex.elementID[e])));
                            Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        Value", size.elementValue[e], value.elementValue[e], hexSplit(hex.elementValue[e])));
                        }

                        pos += GetValueNumeric(hex.xbElementCount[i]);
                    }
                }
                else // IF Codec 8 Extended over TCP
                {
                    for (int i = 0; i < hex.xbElementCount.Length; i++)
                    {
                        if (i == hex.xbElementCount.Length - 1)
                        {
                            Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        xb Element count", size.xbElementCount[i], value.xbElementCount[i], hexSplit(hex.xbElementCount[i])));
                        }
                        else
                        {
                            Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        " + (Math.Pow(2, i)) + "b Element count", size.xbElementCount[i], value.xbElementCount[i], hexSplit(hex.xbElementCount[i])));
                        }

                        for (int e = pos; e < pos + GetValueNumeric(hex.xbElementCount[i]); e++)
                        {
                            Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        ID", size.elementID[e], value.elementID[e], hexSplit(hex.elementID[e])));
                            Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        Value", size.elementValue[e], value.elementValue[e], hexSplit(hex.elementValue[e])));
                        }

                        pos += GetValueNumeric(hex.xbElementCount[i]);
                    }
                }

                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "    AVL Data Count", size.numberOfData2, value.numberOfData2, hexSplit(hex.numberOfData2)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "  Crc", size.crc16, value.crc16, hexSplit(hex.crc16)));
                Console.WriteLine("-------------------------------------------------------------------------------------------------------------");
            }
            else if (hex.protocol == "01") // IF UDP
            {
                Console.WriteLine("-------------------------------------------------------------------------------------------------------------");
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "Name", "Size", "Value", "Hex Value"));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}", "UDP AVL Data Packet", "var"));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "  Length", size.packetLength, value.packetLength, hexSplit(hex.packetLength)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "  Packed ID", size.packetID, value.packetID, hexSplit(hex.packetID)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "  Packet Type", size.packetType, value.packetType, hexSplit(hex.packetType)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "  AVL packet ID", size.avlPacketID, value.avlPacketID, hexSplit(hex.avlPacketID)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "  Imei length", size.imeiLength, value.imeiLength, hexSplit(hex.imeiLength)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "  Imei", size.moduleImei, value.moduleImei, hexSplit(hex.moduleImei)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}", "  Data", "var"));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "    Codec ID", size.codecID, value.codecID, hexSplit(hex.codecID)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "    AVL Data Count", size.numberOfData1, value.numberOfData1, hexSplit(hex.numberOfData1)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}", "    AVL Data", "var"));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "      Timestamp", size.timestamp, value.timestamp, hexSplit(hex.timestamp)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "      Priority", size.priority, value.priority, hexSplit(hex.priority)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}", "      GPS Element", "15"));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        Longitude", size.longitude, value.longitude, hexSplit(hex.longitude)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        Latitude", size.latitude, value.latitude, hexSplit(hex.latitude)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        Altitude", size.altitude, value.altitude, hexSplit(hex.altitude)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        Angle", size.angle, value.angle, hexSplit(hex.angle)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        Satellites", size.satellites, value.satellites, hexSplit(hex.satellites)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        Speed", size.speed, value.speed, hexSplit(hex.speed)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}", "      I/O Element", "var"));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        Event ID", size.eventID, value.eventID, hexSplit(hex.eventID)));
                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        Element count", size.numberOfElements, value.numberOfElements, hexSplit(hex.numberOfElements)));
                int pos = 0;
                if (hex.codecID == "08") // IF Codec 8 over UDP
                {
                    for (int i = 0; i < hex.xbElementCount.Length; i++)
                    {
                        Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        " + (Math.Pow(2, i)) + "b Element count", size.xbElementCount[i], value.xbElementCount[i], hexSplit(hex.xbElementCount[i])));

                        for (int e = pos; e < pos + GetValueNumeric(hex.xbElementCount[i]); e++)
                        {
                            Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        ID", size.elementID[e], value.elementID[e], hexSplit(hex.elementID[e])));
                            Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        Value", size.elementValue[e], value.elementValue[e], hexSplit(hex.elementValue[e])));
                        }

                        pos += GetValueNumeric(hex.xbElementCount[i]);
                    }
                }
                else // IF Codec 8 Extended over UDP
                {
                    for (int i = 0; i < hex.xbElementCount.Length; i++)
                    {
                        if (i == hex.xbElementCount.Length - 1)
                        {
                            Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        xb Element count", size.xbElementCount[i], value.xbElementCount[i], hexSplit(hex.xbElementCount[i])));
                        }
                        else
                        {
                            Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        " + (Math.Pow(2, i)) + "b Element count", size.xbElementCount[i], value.xbElementCount[i], hexSplit(hex.xbElementCount[i])));
                        }

                        for (int e = pos; e < pos + GetValueNumeric(hex.xbElementCount[i]); e++)
                        {
                            Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        ID", size.elementID[e], value.elementID[e], hexSplit(hex.elementID[e])));
                            Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "        Value", size.elementValue[e], value.elementValue[e], hexSplit(hex.elementValue[e])));
                        }

                        pos += GetValueNumeric(hex.xbElementCount[i]);
                    }
                }

                Console.WriteLine(String.Format("{0," + cw1 + "}{1," + cw2 + "}{2," + cw3 + "}{3," + cw4 + "}", "    AVL Data Count", size.numberOfData2, value.numberOfData2, hexSplit(hex.numberOfData2)));
                Console.WriteLine("-------------------------------------------------------------------------------------------------------------");
            }
            else
            {
                Console.WriteLine("Corrupted data inserted.");
            }
        }

        static string hexSplit(string x)
        {
            return Regex.Replace(x, ".{2}(?!$)", "$0-");
        }

        static void Main(string[] args)
        {
            InputHandler();
        }
    }
}
