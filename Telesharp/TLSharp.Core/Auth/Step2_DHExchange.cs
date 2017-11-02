﻿using System;
using System.Collections.Generic;
using System.IO;
using TLSharp.Core.MTProto;
using TLSharp.Core.MTProto.Crypto;

namespace TLSharp.Core.Auth
{
    public class Step2_Response
    {
        public byte[] Nonce { get; set; }
        public byte[] ServerNonce { get; set; }
        public byte[] NewNonce { get; set; }
        public byte[] EncryptedAnswer { get; set; }
    }

    public class Step2_DHExchange
    {
        public byte[] newNonce;

        public Step2_DHExchange()
        {
            newNonce = new byte[32];
        }

        public byte[] ToBytes(byte[] nonce, byte[] serverNonce, List<byte[]> fingerprints, BigInteger pq)
        {
            new Random().NextBytes(newNonce);

            var pqPair = Factorizator.Factorize(pq);

            byte[] reqDhParamsBytes;

            using (MemoryStream pqInnerData = new MemoryStream(255))
            {
                using (TBinaryWriter pqInnerDataWriter = new TBinaryWriter(pqInnerData))
                {
                    pqInnerDataWriter.Write(0x83c95aec); // pq_inner_data
                    pqInnerDataWriter.Write(pq.ToByteArrayUnsigned());
                    pqInnerDataWriter.Write(pqPair.Min.ToByteArrayUnsigned());
                    pqInnerDataWriter.Write(pqPair.Max.ToByteArrayUnsigned());
                    pqInnerDataWriter.WriteBase(nonce);
                    pqInnerDataWriter.WriteBase(serverNonce);
                    pqInnerDataWriter.WriteBase(newNonce);

                    byte[] ciphertext = null;
                    byte[] targetFingerprint = null;
                    foreach (byte[] fingerprint in fingerprints)
                    {
                        ciphertext = RSA.Encrypt(BitConverter.ToString(fingerprint).Replace("-", string.Empty),
                                                 pqInnerData.GetBuffer(), 0, (int)pqInnerData.Position);
                        if (ciphertext != null)
                        {
                            targetFingerprint = fingerprint;
                            break;
                        }
                    }

                    if (ciphertext == null)
                    {
                        throw new InvalidOperationException(
                            String.Format("not found valid key for fingerprints: {0}", String.Join(", ", fingerprints)));
                    }

                    using (MemoryStream reqDHParams = new MemoryStream(1024))
                    {
                        using (TBinaryWriter reqDHParamsWriter = new TBinaryWriter(reqDHParams))
                        {
                            reqDHParamsWriter.Write(0xd712e4be); // req_dh_params
                            reqDHParamsWriter.WriteBase(nonce);
                            reqDHParamsWriter.WriteBase(serverNonce);
                            reqDHParamsWriter.Write(pqPair.Min.ToByteArrayUnsigned());
                            reqDHParamsWriter.Write(pqPair.Max.ToByteArrayUnsigned());
                            reqDHParamsWriter.WriteBase(targetFingerprint);
                            reqDHParamsWriter.Write(ciphertext);

                            reqDhParamsBytes = reqDHParams.ToArray();
                        }
                    }
                }
                return reqDhParamsBytes;
            }
        }

        public Step2_Response FromBytes(byte[] response)
        {
            byte[] encryptedAnswer;

            using (MemoryStream responseStream = new MemoryStream(response, false))
            {
                using (TBinaryReader responseReader = new TBinaryReader(responseStream))
                {
                    uint responseCode = responseReader.ReadUInt32();

                    if (responseCode == 0x79cb045d)
                    {
                        // server_DH_params_fail
                        throw new InvalidOperationException("server_DH_params_fail: TODO");
                    }

                    if (responseCode != 0xd0e8075c)
                    {
                        throw new InvalidOperationException($"invalid response code: {responseCode}");
                    }

                    byte[] nonceFromServer = responseReader.ReadBytes(16);
                    byte[] serverNonceFromServer = responseReader.ReadBytes(16);

                    encryptedAnswer = responseReader.ReadBytes();

                    return new Step2_Response()
                    {
                        EncryptedAnswer = encryptedAnswer,
                        ServerNonce = serverNonceFromServer,
                        Nonce = nonceFromServer,
                        NewNonce = newNonce
                    };
                }
            }
        }
    }
}