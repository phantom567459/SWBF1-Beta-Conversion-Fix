using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Enumeration;

namespace BetaConversionFixBF1
{
    class Program
    {
        static void Main(string[] args)
        {
            bool reverseFlag = false;
            string fileName, fileName2;
            //Console.WriteLine("Hello World!");

            if (args.Length < 2) {
                Console.WriteLine("Please enter argument in this fashion - program.exe <NAME-IN> <NAME-OUT>");
                Console.WriteLine("-r can be added to the end as an experimental reverse flag");
                return; //stop further execution
            }
            Console.WriteLine("Arguments:");
            fileName = args[0];
            fileName2 = args[1];

            if (args.Length != 2)
            {
                if (args[2] == "-r")
                {
                    reverseFlag = true;
                }
                else
                {
                    Console.WriteLine("Please enter argument in this fashion - program.exe <NAME-IN> <NAME-OUT>");
                    Console.WriteLine("-r can be added to the end as an experimental reverse flag");
                    return; //stop further execution
                }
            }

            byte[] ByteBuffer = File.ReadAllBytes(fileName);
            byte[] StringBytes = Encoding.ASCII.GetBytes("modl");
            byte[] StringINFO = Encoding.ASCII.GetBytes("INFO");

            byte[] EditBytes;
            byte[] Result = { };
            byte[] finalFile = { };
            byte[] array1 = { };
            byte[] array2 = { 0, 0, 0, 0 };

            int modlCount = 0;
            Int32 modlSize = 0;
            Int32 infoSize = 0;

            BinaryReader binReader = new BinaryReader(File.Open(fileName, FileMode.Open));
            Encoding ascii = Encoding.ASCII;
            BinaryWriter bwEncoder = new BinaryWriter(new FileStream(fileName2, FileMode.Create), ascii);

            for (int i = 0; i <= (ByteBuffer.Length - StringBytes.Length); i++)
            {
                if (ByteBuffer[i] == StringBytes[0])
                {
                    for (int j = 1; j < StringBytes.Length && ByteBuffer[i + j] == StringBytes[j]; j++)
                    {
                        if (j == 3) //set to 3 to actually find the right string
                        {
                            modlCount += 1;
                            binReader.BaseStream.Position = i + 4;
                            modlSize = binReader.ReadInt32();
                            if (modlSize > ByteBuffer.Length){
                                continue;
                            }
                            binReader.BaseStream.Position = i;
                            Console.WriteLine("modl data found at offset {0}, num {1}, size {2}", i, modlCount,modlSize);
                            EditBytes = binReader.ReadBytes(Convert.ToInt32(modlSize) + 8);
                            for (int k = 0; k <= (EditBytes.Length - StringINFO.Length); k++)
                            {
                                if (EditBytes[k] == StringINFO[0])
                                {
                                    for (int q = 1; q < StringINFO.Length && EditBytes[k + q] == StringINFO[q]; q++)
                                    {
                                        if (q == 3)
                                        {
                                            Console.WriteLine("INFO data found at offset {0} of subsection", k);
                                           // if (reverseFlag == false)
                                           // {
                                                infoSize = BitConverter.ToInt32(EditBytes, k + 4);
                                          /*  }
                                            else if (reverseFlag == true)
                                            {
                                                infoSize = BitConverter.ToInt32(EditBytes, k - 4);
                                            }*/

                                            Console.WriteLine(infoSize);
                                            if (infoSize == 64 & reverseFlag == false)
                                            {
                                                array1 = BitConverter.GetBytes(Convert.ToInt32(infoSize + 4));
                                                Array.Resize(ref array1, array1.Length + 4);
                                                System.Buffer.BlockCopy(array2, 0, array1, 4, array2.Length);
                                                //if (modlCount == 1)
                                                //{
                                                Result = Replace(EditBytes, StringINFO.Concat(BitConverter.GetBytes(infoSize)).ToArray(), StringINFO.Concat(array1).ToArray());
                                                Result = Replace(Result, StringBytes.Concat(BitConverter.GetBytes(modlSize)).ToArray(), StringBytes.Concat(BitConverter.GetBytes(modlSize + 4)).ToArray());
                                                // }
                                                // else
                                                // {
                                                //     Result = Replace(Result, BitConverter.GetBytes(infoSize), array1);
                                                // }
                                                Array.Clear(array1,0,array1.Length);
                                                break;
                                            }
                                            else if (infoSize == 68 & reverseFlag == true)
                                            {
                                                array1 = BitConverter.GetBytes(Convert.ToInt32(infoSize - 4));
                                                //Array.Resize(ref array2, 0);
                                               // System.Buffer.BlockCopy(array1, 0, array3, 0, infoSize - 4);
                                                //System.Buffer.BlockCopy(array2, 0, array1, 4, array2.Length);
                                                Result = Replace(EditBytes, StringINFO.Concat(BitConverter.GetBytes(infoSize)).ToArray(), StringINFO.Concat(array2).ToArray());
                                                Result = Replace(Result, StringINFO.Concat(array2).Concat(array2).ToArray(), StringINFO.Concat(array1).ToArray());
                                                Result = Replace(Result, StringBytes.Concat(BitConverter.GetBytes(modlSize)).ToArray(), StringBytes.Concat(BitConverter.GetBytes(modlSize - 4)).ToArray());
                                                Array.Clear(array1, 0, array1.Length);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            if (modlCount == 1)
                            {
                                finalFile = Replace(ByteBuffer, EditBytes, Result);
                            }
                            else
                            {
                                finalFile = Replace(finalFile, EditBytes, Result);
                            }
                        }
                    }
                }
            }
            binReader.Close();
            bwEncoder.Close();
            File.WriteAllBytes(fileName2, finalFile);
        }

        //byte handlers
        private static byte[] Replace(byte[] input, byte[] pattern, byte[] replacement)
        {
            if (pattern.Length == 0)
            {
                return input;
            }

            List<byte> result = new List<byte>();

            int i;

            for (i = 0; i <= input.Length - pattern.Length; i++)
            {
                bool foundMatch = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (input[i + j] != pattern[j])
                    {
                        foundMatch = false;
                        break;
                    }
                }

                if (foundMatch)
                {
                    result.AddRange(replacement);
                    i += pattern.Length - 1;
                }
                else
                {
                    result.Add(input[i]);
                }
            }

            for (; i < input.Length; i++)
            {
                result.Add(input[i]);
            }

            return result.ToArray();
        }
        public static int FindBytes(byte[] src, byte[] find)
        {
            int index = -1;
            int matchIndex = 0;
            // handle the complete source array
            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == find[matchIndex])
                {
                    if (matchIndex == (find.Length - 1))
                    {
                        index = i - matchIndex;
                        break;
                    }
                    matchIndex++;
                }
                else if (src[i] == find[0])
                {
                    matchIndex = 1;
                }
                else
                {
                    matchIndex = 0;
                }

            }
            return index;
        }
    }


}
