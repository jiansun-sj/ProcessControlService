using System;


namespace ProcessControlService.ResourceLibrary.Machines.Drivers
{
    public class FanucCNCDriver
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(FanucCNCDriver));

        #region Private fields

        string IP;
        public bool conn = false;
        ushort Flibhndl = 0;
        #endregion

        #region Constructor
        public FanucCNCDriver(string ip)
        {
            IP = ip;
        }
        #endregion

        public bool Connect()
        {
            ushort h = 0;
            short ret = Focas1.cnc_allclibhndl3(IP, 8193, 3, out h);
            if (ret == Focas1.EW_OK)//连接成功
            {
                conn = true;
                Flibhndl = h;
                return true;
            }
            else
            {
                conn = false;
                return false;
            }
        }

        public void Disconnect()
        {
            conn = false;
            Focas1.cnc_freelibhndl(Flibhndl);
        }

        public object ReadItem(string address)
        {
            lock (this)
            {
                try
                {
                    CNCAddress addr = CNCAddress.ConvertAddress(address);
                    if (addr.AddrType == "Y")
                    {
                        short ret = GetYValue(addr.BlockID);

                        if (addr.ValType == "Bool")
                        {
                            return Convert.ToBoolean(ret & Convert.ToByte(Math.Pow(2, addr.BitOffset)));
                        }
                        else
                        {
                            return ret;
                        }
                    }
                    else if (addr.AddrType == "R")
                    {
                        short val = 0;
                        short ret = ReadPmcRAddr(Convert.ToUInt16(addr.BlockID), out val);
                        if (ret == -16)
                        {
                            throw new Exception("CNC读取R出错" + ret);
                        }
                        else
                        {
                            if (addr.ValType == "Bool")
                            {
                                return Convert.ToBoolean(val & Convert.ToByte(Math.Pow(2, addr.BitOffset)));
                            }
                            else
                            {
                                return val;
                            }
                        }
                    }
                    else if (addr.AddrType == "F")
                    {
                        short val = 0;
                        short ret = ReadPmcFAddr(Convert.ToUInt16(addr.BlockID), out val);
                        if (ret == -16)
                        {
                            throw new Exception("CNC读取F出错" + ret);
                        }
                        else
                        {
                            if (addr.ValType == "Bool")
                            {
                                return Convert.ToBoolean(val & Convert.ToByte(Math.Pow(2, addr.BitOffset)));
                            }
                            else
                            {
                                return val;
                            }
                        }
                    }
                    else if (addr.AddrType == "X")
                    {
                        short val = 0;
                        short ret = ReadPmcXAddr(Convert.ToUInt16(addr.BlockID), out val);
                        if (ret == -16)
                        {
                            throw new Exception("CNC读取X出错" + ret);
                        }
                        else
                        {
                            if (addr.ValType == "Bool")
                            {
                                return Convert.ToBoolean(val & Convert.ToByte(Math.Pow(2, addr.BitOffset)));
                            }
                            else
                            {
                                return val;
                            }
                        }
                    }
                    else if (addr.AddrType == "K")
                    {
                        short val = 0;
                        short ret = ReadPmcKAddr(Convert.ToUInt16(addr.BlockID), out val);
                        if (ret == -16)
                        {
                            throw new Exception("CNC读取K出错" + ret);
                        }
                        else
                        {
                            if (addr.ValType == "Bool")
                            {
                                return Convert.ToBoolean(val & Convert.ToByte(Math.Pow(2, addr.BitOffset)));
                            }
                            else
                            {
                                return val;
                            }
                        }
                    }
                    else if (addr.AddrType == "M")
                    {
                        short val = 0;
                        short ret = ReadPmcMacroAddr(Convert.ToInt16(addr.BlockID), out val);
                        if (ret == -16)
                        {
                            throw new Exception("CNC读取K出错" + ret);
                        }
                        else
                        {
                            if (addr.ValType == "Bool")
                            {
                                return Convert.ToBoolean(val & Convert.ToByte(Math.Pow(2, addr.BitOffset)));
                            }
                            else
                            {
                                return val;
                            }
                        }
                    }
                    else if (addr.AddrType == "A")
                    {
                        short val = 0;
                        short ret = ReadPmcAAddr(Convert.ToUInt16(addr.BlockID), out val);
                        if (ret == -16)
                        {
                            throw new Exception("CNC读取A出错" + ret);
                        }
                        else
                        {
                            if (addr.ValType == "Bool")
                            {
                                return Convert.ToBoolean(val & Convert.ToByte(Math.Pow(2, addr.BitOffset)));
                            }
                            else
                            {
                                return val;
                            }
                        }
                    }
                    else if (addr.AddrType == "P")
                    {
                        return ReadParameterValue(Convert.ToInt16(addr.ParaOffset));
                    }
                    else if (addr.AddrType == "V")
                    {
                        return ReadPmcAlarm();
                    }
                    else
                    {
                        throw new Exception("地址错误");
                    }
                }
                catch (Exception ex)
                {
                    LOG.Error(string.Format("CNC Driver 读数据出错：{0}", ex.Message));
                    return null;
                }

            }
        }

        public void WriteItems(Tag tag, object value)
        {
            lock (this)
            {
                try
                {
                    CNCAddress addr = CNCAddress.ConvertAddress(tag.Address);
                    //if (addr.ValType == "Bool" || tag.TagType =="bool")
                    if (addr.ValType == "Bool")
                    {
                        if (addr.AddrType == "Y")
                        {
                            WritePmcYAddrBool(addr.BlockID, addr.BitOffset, Convert.ToBoolean(value));
                        }
                        else
                        {
                            throw new Exception("暂时不支持该地址的bool类型的写入");
                        }
                    }
                    else
                    {
                        if (addr.AddrType == "R")
                        {
                            WritePmcRAddr(addr.BlockID, Convert.ToInt16(value));
                        }
                        else if (addr.AddrType == "K")
                        {
                            WritePmcKAddr(addr.BlockID, Convert.ToInt16(value));
                        }
                        else if (addr.AddrType == "M")
                        {
                            WritePmcMacroAddr(Convert.ToInt16(addr.BlockID), Convert.ToInt32(value));
                        }
                        else
                        {
                            throw new Exception("暂时不支持该类型地址的写入");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LOG.Error(string.Format("CNC Driver 写数据出错：{0}", ex.Message));
                }
            }

        }

        #region function
        //返回-16代表未获取成功
        //读Y地址
        private short GetYValue(ushort YAddr)
        {
            Focas1.IODBPMC0 pmcdata1 = new Focas1.IODBPMC0(); // for 1 Byte

            short ret = Focas1.pmc_rdpmcrng(Flibhndl, 2, 0, YAddr, YAddr, 9, pmcdata1); // D data of 1 Byte
            if (ret == 0)
            {
                short value = pmcdata1.cdata[0];
                return value;
            }
            else
            {
                throw new Exception("CNC读取Y地址出错" + ret);
            }

        }
        //写Y地址
        private short WritePmcYAddrBool(ushort nAddr, byte BitOffset, bool nRValue)
        {
            ushort _Flibhndl = 0;
            //建立连接
            short ret = Focas1.cnc_allclibhndl3(IP, 8193, 3, out _Flibhndl);
            if (ret == Focas1.EW_OK)//连接成功
            {
                Focas1.IODBPMC0 info = new Focas1.IODBPMC0();
                //ret = Focas1.pmc_rdpmcrng(_Flibhndl, 9, 0, 6000, 6000, 9, info); //
                //int flag = (info.cdata[0] & 2) == 2 ? 1 : 0; //d6000.1      .0是1/.1是2/.2是4/6/8
                //if (flag == 0)
                //{
                //    info.cdata[0] += 2;  //这是写1的情况，写0的话就是反逻辑
                //}

                byte BitOffset2 = Convert.ToByte(Math.Pow(2, BitOffset));
                ret = Focas1.pmc_rdpmcrng(_Flibhndl, 2, 0, nAddr, nAddr, 9, info); //Y地址是2
                bool flag = (info.cdata[0] & BitOffset2) == BitOffset2 ? true : false;

                if (nRValue)  //要写入的是true
                {
                    if (!flag) //当前值是false
                    {
                        info.cdata[0] += BitOffset2;
                    }
                }
                else   //要写入的值是false
                {
                    if (flag) //当前值是true
                    {
                        info.cdata[0] -= BitOffset2;
                    }
                }

                ret = Focas1.pmc_wrpmcrng(_Flibhndl, 9, info);
                if (ret == 0)
                {
                    Focas1.cnc_freelibhndl(_Flibhndl);
                    return ret;
                }
                else
                {
                    Focas1.cnc_freelibhndl(_Flibhndl);
                    throw new Exception("CNC写Y地址写入出错" + ret);
                }

            }
            else
            {
                throw new Exception("CNC写Y地址出错连接失败" + ret);
            }
        }

        // Read Parameters
        private int ReadParameterValue(short para)
        {

            Focas1.IODBPSD_1 Param = new Focas1.IODBPSD_1();
            short nRet2 = Focas1.cnc_rdparam(Flibhndl, para, -1, 4 + Focas1.MAX_AXIS, Param);
            if (nRet2 == 0)
            {
                return Param.ldata;
            }
            else
            {
                throw new Exception("CNC读取Parameter出错" + nRet2);
            }

        }

        //写R地址
        private void WritePmcRAddr(ushort nAddr, short nRValue)
        {
            //建立连接
            var ret = Focas1.cnc_allclibhndl3(IP, 8193, 3, out var flibhndl);
            if (ret == Focas1.EW_OK)//连接成功
            {
                Focas1.IODBPMC0 pmcdata1 = new Focas1.IODBPMC0(); // for 1 Byte

                ret = Focas1.pmc_rdpmcrng(flibhndl, 5, 0, nAddr, nAddr, 9, pmcdata1); // D data of 1 Byte

                short value = pmcdata1.cdata[0];
                pmcdata1.cdata[0] = Convert.ToByte(nRValue);
                ret = Focas1.pmc_wrpmcrng(flibhndl, 9, pmcdata1);
                if (ret==0)
                {
                    Focas1.cnc_freelibhndl(flibhndl);
                    return;
                }
                else
                {
                    Focas1.cnc_freelibhndl(flibhndl);
                    throw new Exception("CNC写R地址写入出错" + ret);
                }

            }
            else
            {
                throw new Exception("CNC写R地址出错连接失败" + ret);
            }
            //Focas1.IODBPMC0 pmcdata1 = new Focas1.IODBPMC0(); // for 1 Byte

            //short ret = Focas1.pmc_rdpmcrng(Flibhndl, 5, 0, nAddr, nAddr, 9, pmcdata1); // D data of 1 Byte
            //if (ret == 0)
            //{
            //    short value = pmcdata1.cdata[0];
            //    pmcdata1.cdata[0] = Convert.ToByte(nRValue);
            //    ret = Focas1.pmc_wrpmcrng(Flibhndl, nAddr, pmcdata1);
            //    if (ret == 0)
            //    {
            //        return ret;
            //    }
            //    else
            //    {
            //        throw new Message("CNC写地址出错" + ret);
            //    }
            //}
            //else
            //{
            //    throw new Message("CNC写地址出错" + ret);
            //}

        }

        //读报警号-类型是V，地址是0
        private int ReadPmcAlarm()
        {
            Focas1.ODBALMMSG2 odbalmmsg2 = new Focas1.ODBALMMSG2();
            short num = 5;
            short ret = Focas1.cnc_rdalmmsg2(Flibhndl, 8, ref num, odbalmmsg2);
            if (ret == 0)
            {
                int almno = odbalmmsg2.msg1.alm_no;
                return almno;
            }
            else
            {
                throw new Exception("CNC读取报警号出错" + ret);
            }

        }
        
        //读R地址
        private short ReadPmcRAddr(ushort nAddr, out short nRValue)
        {
            Focas1.IODBPMC0 pmcdata1 = new Focas1.IODBPMC0(); // for 1 Byte

            short ret = Focas1.pmc_rdpmcrng(Flibhndl, 5, 0, nAddr, nAddr, 9, pmcdata1); // D data of 1 Byte
            if (ret == 0)
            {
                short value = pmcdata1.cdata[0];
                nRValue = Convert.ToInt16(pmcdata1.cdata[0]);
                return ret;
            }
            else
            {
                throw new Exception("CNC读取R地址出错" + ret);
            }

        }
        //读F地址
        private short ReadPmcFAddr(ushort nAddr, out short nRValue)
        {
            Focas1.IODBPMC0 pmcdata1 = new Focas1.IODBPMC0(); // for 1 Byte

            short ret = Focas1.pmc_rdpmcrng(Flibhndl, 1, 0, nAddr, nAddr, 9, pmcdata1); // D data of 1 Byte
            if (ret == 0)
            {
                short value = pmcdata1.cdata[0];
                nRValue = Convert.ToInt16(pmcdata1.cdata[0]);
                return ret;
            }
            else
            {
                throw new Exception("CNC读取F地址出错" + ret);
            }

        }
        //读X地址
        private short ReadPmcXAddr(ushort nAddr, out short nRValue)
        {
            Focas1.IODBPMC0 pmcdata1 = new Focas1.IODBPMC0(); // for 1 Byte

            short ret = Focas1.pmc_rdpmcrng(Flibhndl, 3, 0, nAddr, nAddr, 9, pmcdata1); // D data of 1 Byte
            if (ret == 0)
            {
                short value = pmcdata1.cdata[0];
                nRValue = Convert.ToInt16(pmcdata1.cdata[0]);
                return ret;
            }
            else
            {
                throw new Exception("CNC读取X地址出错" + ret);
            }

        }

        //读M_macro地址
        private short ReadPmcMacroAddr(short nAddr, out short nRValue)
        {
            Focas1.ODBM macro = new Focas1.ODBM();
            short ret = Focas1.cnc_rdmacro(Flibhndl, nAddr, 10, macro);
            if (ret == Focas1.EW_OK)
            {
                //****************
                // int nRet = -16;
                double dSmall = macro.dec_val * 1.0;
                double top_prog = macro.mcr_val / Math.Pow(10.0, dSmall);
                nRValue = Convert.ToInt16(top_prog);
                //short num_prog = 1;
                return ret;
            }
            else
            {
                throw new Exception("CNC读取M_macro地址出错" + ret);
            }
        }
        //写M_macro地址
        private short WritePmcMacroAddr(short nAddr, int nRValue)
        {
            ushort _Flibhndl = 0;
            //建立连接
            short ret = Focas1.cnc_allclibhndl3(IP, 8193, 3, out _Flibhndl);
            if (ret == Focas1.EW_OK)//连接成功
            {
                ret = Focas1.cnc_wrmacro(_Flibhndl, nAddr, 10, nRValue*10000, 4);
                if (ret == 0)
                {
                    Focas1.cnc_freelibhndl(_Flibhndl);
                    return ret;
                }
                else
                {
                    Focas1.cnc_freelibhndl(_Flibhndl);
                    throw new Exception("CNC写M_macro地址写入出错" + ret);
                }

            }
            else
            {
                throw new Exception("CNC写M_macro地址出错连接失败" + ret);
            }
        }

        //读K地址
        private short ReadPmcKAddr(ushort nAddr, out short nRValue)
        {
            Focas1.IODBPMC0 pmcdata1 = new Focas1.IODBPMC0(); // for 1 Byte

            short ret = Focas1.pmc_rdpmcrng(Flibhndl, 7, 0, nAddr, nAddr, 9, pmcdata1); // D data of 1 Byte
            if (ret == 0)
            {
                short value = pmcdata1.cdata[0];
                nRValue = Convert.ToInt16(pmcdata1.cdata[0]);
                return ret;
            }
            else
            {
                throw new Exception("CNC读取K地址出错" + ret);
            }

        }
        //写K地址
        private short WritePmcKAddr(ushort nAddr, short nRValue)
        {
            ushort _Flibhndl = 0;
            //建立连接
            short ret = Focas1.cnc_allclibhndl3(IP, 8193, 3, out _Flibhndl);
            if (ret == Focas1.EW_OK)//连接成功
            {
                Focas1.IODBPMC0 pmcdata1 = new Focas1.IODBPMC0(); // for 1 Byte

                ret = Focas1.pmc_rdpmcrng(_Flibhndl, 7, 0, nAddr, nAddr, 9, pmcdata1); // D data of 1 Byte

                short value = pmcdata1.cdata[0];
                pmcdata1.cdata[0] = Convert.ToByte(nRValue);
                ret = Focas1.pmc_wrpmcrng(_Flibhndl, 9, pmcdata1);
                if (ret == 0)
                {
                    Focas1.cnc_freelibhndl(_Flibhndl);
                    return ret;
                }
                else
                {
                    Focas1.cnc_freelibhndl(_Flibhndl);
                    throw new Exception("CNC写K地址写入出错" + ret);
                }

            }
            else
            {
                throw new Exception("CNC写K地址出错连接失败" + ret);
            }
        }
        //读A地址
        private short ReadPmcAAddr(ushort nAddr, out short nRValue)
        {
            Focas1.IODBPMC0 pmcdata1 = new Focas1.IODBPMC0(); // for 1 Byte

            short ret = Focas1.pmc_rdpmcrng(Flibhndl, 4, 0, nAddr, nAddr, 9, pmcdata1); // D data of 1 Byte
            if (ret == 0)
            {
                short value = pmcdata1.cdata[0];
                nRValue = Convert.ToInt16(pmcdata1.cdata[0]);
                return ret;
            }
            else
            {
                throw new Exception("CNC读取A地址出错" + ret);
            }

        }
        #endregion

    }

    public class CNCAddress
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(CNCAddress));

        //public string CompleteAddress;   //DB200.DBX1.1
        public string ValType;              // bool
        public ushort BlockID;          // 200
        public byte BitOffset = 0;         // 1
        public ushort ParaOffset = 0;

        public string AddrType;

        public static CNCAddress ConvertAddress(string Address)
        {
            CNCAddress cncaddr = new CNCAddress();
            if (Address.Contains("."))
            {
                cncaddr.ValType = "Bool";
                cncaddr.AddrType = Address.Substring(0, 1).ToUpper();
                string add = Address.Substring(1, Address.Length - 1);
                string[] x = add.Split('.');
                cncaddr.BlockID = Convert.ToUInt16(x[0]);
                cncaddr.BitOffset = Convert.ToByte(x[1]);
                if (cncaddr.BitOffset>7)
                {
                    throw new Exception("bool类型的地址位不能大于7");
                }
            }
            else if (Address.ToLower().Contains("r"))
            {
                cncaddr.AddrType = "R";
                string offset = Address.Substring(1, Address.Length - 1);
                cncaddr.BlockID = Convert.ToUInt16(offset);
            }
            else if (Address.ToLower().Contains("f"))
            {
                cncaddr.AddrType = "F";
                string offset = Address.Substring(1, Address.Length - 1);
                cncaddr.BlockID = Convert.ToUInt16(offset);
            }
            else if (Address.ToLower().Contains("x"))
            {
                cncaddr.AddrType = "X";
                string offset = Address.Substring(1, Address.Length - 1);
                cncaddr.BlockID = Convert.ToUInt16(offset);
            }
            else if (Address.ToLower().Contains("k"))
            {
                cncaddr.AddrType = "K";
                string offset = Address.Substring(1, Address.Length - 1);
                cncaddr.BlockID = Convert.ToUInt16(offset);
            }
            else if (Address.ToLower().Contains("a"))
            {
                cncaddr.AddrType = "A";
                string offset = Address.Substring(1, Address.Length - 1);
                cncaddr.BlockID = Convert.ToUInt16(offset);
            }
            else if (Address.ToLower().Contains("m"))
            {
                cncaddr.AddrType = "M";
                string offset = Address.Substring(1, Address.Length - 1);
                cncaddr.BlockID = Convert.ToUInt16(offset);
            }
            else if (Address.ToLower().Contains("y"))
            {
                string offset = Address.Substring(1, Address.Length - 1);
                cncaddr.BlockID = Convert.ToUInt16(offset);
                cncaddr.AddrType = "Y";
            }
            else if (Address.ToLower().Contains("p"))
            {
                cncaddr.AddrType = "P";
                string offset = Address.Substring(1, Address.Length - 1);
                cncaddr.ParaOffset = Convert.ToUInt16(offset);
            }
            else if (Address.ToLower().Contains("v"))
            {
                cncaddr.AddrType = "V";
            }
            else
            {
                LOG.Error(string.Format("CNCAddress 地址不识别出错: {0}", Address));
            }

            return cncaddr;
        }
    }

}


