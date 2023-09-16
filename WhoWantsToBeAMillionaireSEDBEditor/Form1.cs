using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Xml;

namespace WhoWantsToBeAMillionaireSEDBEditor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static Settings settings = new Settings(1251, true);

        List<f3pStructure> ListQuestions = new List<f3pStructure>();
        List<mqpStructure> CommonListQuestions = new List<mqpStructure>();
        List<mqpBlocks> BlockList = new List<mqpBlocks>();


        string GetExtension = "";
        string FullName = "";
        byte[] binContent;

        public class mqpBlocks
        {
            public string type;
            public int counter;
            public uint block_offset;
            public int block_length;

            public mqpBlocks() { }

            public mqpBlocks(string _type, int _counter,
                uint _block_offset, int _block_length)
            {
                this.type = _type;
                this.counter = _counter;
                this.block_offset = _block_offset;
                this.block_length = _block_length;
            }
        }

        public class mqpStructure
        {
            public string question, a_answer, b_answer, c_answer, d_answer;
            public int block_num;

            public mqpStructure(){}

            public mqpStructure(string _question, string _a_answer, string _b_answer,
                string _c_answer, string _d_answer, int _block_num)
            {
                this.question = _question;
                this.a_answer = _a_answer;
                this.b_answer = _b_answer;
                this.c_answer = _c_answer;
                this.d_answer = _d_answer;
                this.block_num = _block_num;
            }
        }

        public class f3pStructure
        {
            public string question, a_answer, b_answer, c_answer, d_answer;

            public f3pStructure(){}
            public f3pStructure(string _question, string _a_answer,
                string _b_answer, string _c_answer, string _d_answer)
            {
                this.question = _question;
                this.a_answer = _a_answer;
                this.b_answer = _b_answer;
                this.c_answer = _c_answer;
                this.d_answer = _d_answer;
            }
        }

        

        private void Savef3pFiles(byte[] binContent, string output_path)
        {
            uint GetBegining = 28 + (uint)dataGridView1.RowCount * 32;
            byte[] count_byte = new byte[4];
            byte[] begining_byte = new byte[4];
            count_byte = BitConverter.GetBytes(dataGridView1.RowCount);
            begining_byte = BitConverter.GetBytes(GetBegining);

            uint offset = 8;
            Array.Copy(count_byte, 0, binContent, offset, count_byte.Length);
            offset += 8;
            Array.Copy(begining_byte, 0, binContent, offset, begining_byte.Length);
            offset += 12;

            MemoryStream ms = new MemoryStream();

            uint cur_off = 0;

            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                string question = dataGridView1[0, i].Value.ToString() + "\0";
                string a_var = dataGridView1[1, i].Value.ToString() + "\0";
                string b_var = dataGridView1[2, i].Value.ToString() + "\0";
                string c_var = dataGridView1[3, i].Value.ToString() + "\0";
                string d_var = dataGridView1[4, i].Value.ToString() + "\0";

                question = Methods.ConvertToLatin(question);
                a_var = Methods.ConvertToLatin(a_var);
                b_var = Methods.ConvertToLatin(b_var);
                c_var = Methods.ConvertToLatin(c_var);
                d_var = Methods.ConvertToLatin(d_var);


                byte[] q_bytes = Encoding.Unicode.GetBytes(question);
                byte[] a_bytes = Encoding.Unicode.GetBytes(a_var);
                byte[] b_bytes = Encoding.Unicode.GetBytes(b_var);
                byte[] c_bytes = Encoding.Unicode.GetBytes(c_var);
                byte[] d_bytes = Encoding.Unicode.GetBytes(d_var);

                ms.Write(q_bytes, 0, q_bytes.Length);
                ms.Write(a_bytes, 0, a_bytes.Length);
                ms.Write(b_bytes, 0, b_bytes.Length);
                ms.Write(c_bytes, 0, c_bytes.Length);
                ms.Write(d_bytes, 0, d_bytes.Length);

                byte[] current_off = new byte[4];
                current_off = BitConverter.GetBytes(cur_off);
                uint a_offset = (uint)q_bytes.Length;
                uint b_offset = a_offset + (uint)a_bytes.Length;
                uint c_offset = b_offset + (uint)b_bytes.Length;
                uint d_offset = c_offset + (uint)c_bytes.Length;

                uint next_off = (uint)q_bytes.Length + (uint)a_bytes.Length + (uint)b_bytes.Length + (uint)c_bytes.Length + (uint)d_bytes.Length;
                byte[] next_offset = new byte[4];
                next_offset = BitConverter.GetBytes(next_off);
                byte[] a_off = new byte[4];
                a_off = BitConverter.GetBytes(a_offset);
                byte[] b_off = new byte[4];
                b_off = BitConverter.GetBytes(b_offset);
                byte[] c_off = new byte[4];
                c_off = BitConverter.GetBytes(c_offset);
                byte[] d_off = new byte[4];
                d_off = BitConverter.GetBytes(d_offset);

                cur_off += next_off;

                Array.Copy(current_off, 0, binContent, offset, current_off.Length);
                offset += 4;
                Array.Copy(next_offset, 0, binContent, offset, next_offset.Length);
                offset += 8;
                Array.Copy(a_off, 0, binContent, offset, a_off.Length);
                offset += 4;
                Array.Copy(b_off, 0, binContent, offset, b_off.Length);
                offset += 4;
                Array.Copy(c_off, 0, binContent, offset, c_off.Length);
                offset += 4;
                Array.Copy(d_off, 0, binContent, offset, d_off.Length);
                offset += 8;
            }
            byte[] strings = ms.ToArray();
            ms.Close();

            byte[] new_bin_content = new byte[GetBegining];
            Array.Copy(binContent, 0, new_bin_content, 0, new_bin_content.Length);

            if (File.Exists(output_path)) File.Delete(output_path);
            FileStream fs = new FileStream(output_path, FileMode.CreateNew);
            fs.Write(new_bin_content, 0, new_bin_content.Length);
            fs.Write(strings, 0, strings.Length);
            fs.Close();
        }

        private void Readf3pFiles(byte[] binContent)
        {
            uint begining;
            uint current_offset;
            uint next_offset;

            byte[] begining_bytes = new byte[4];
            Array.Copy(binContent, 16, begining_bytes, 0, 4);
            begining = BitConverter.ToUInt32(begining_bytes, 0);
            current_offset = 0;
            uint a_offset = 0;
            uint b_offset = 0;
            uint c_offset = 0;
            uint d_offset = 0;

            uint offset = 28;

            int max_length_q = 0; //Длина строки вопросов
            int max_length_a = 0; //Длина строки варианта A
            int max_length_b = 0; //Длина строки варианта B
            int max_length_c = 0; //Длина строки варианта C
            int max_length_d = 0; //Длина строки варианта D

            while (offset <= begining)
            {
                byte[] cur_off = new byte[4];
                Array.Copy(binContent, offset, cur_off, 0, 4);
                current_offset = begining + BitConverter.ToUInt32(cur_off, 0);
                offset += 4;
                byte[] next_off = new byte[4];
                Array.Copy(binContent, offset, next_off, 0, 4);
                next_offset = current_offset + BitConverter.ToUInt32(next_off, 0);
                offset += 8;
                byte[] a_offset_byte = new byte[4];
                byte[] b_offset_byte = new byte[4];
                byte[] c_offset_byte = new byte[4];
                byte[] d_offset_byte = new byte[4];
                Array.Copy(binContent, offset, a_offset_byte, 0, 4);
                a_offset = current_offset + BitConverter.ToUInt32(a_offset_byte, 0);
                offset += 4;
                Array.Copy(binContent, offset, b_offset_byte, 0, 4);
                b_offset = current_offset + BitConverter.ToUInt32(b_offset_byte, 0);
                offset += 4;
                Array.Copy(binContent, offset, c_offset_byte, 0, 4);
                c_offset = current_offset + BitConverter.ToUInt32(c_offset_byte, 0);
                offset += 4;
                Array.Copy(binContent, offset, d_offset_byte, 0, 4);
                d_offset = current_offset + BitConverter.ToUInt32(d_offset_byte, 0);
                offset += 8;

                int question_length = (int)a_offset - (int)current_offset - 2;
                int a_length = (int)b_offset - (int)a_offset - 2;
                int b_length = (int)c_offset - (int)b_offset - 2;
                int c_length = (int)d_offset - (int)c_offset - 2;
                int d_length = (int)next_offset - (int)d_offset - 2;
                byte[] question_byte = new byte[question_length];
                byte[] a_byte = new byte[a_length];
                byte[] b_byte = new byte[b_length];
                byte[] c_byte = new byte[c_length];
                byte[] d_byte = new byte[d_length];
                Array.Copy(binContent, current_offset, question_byte, 0, question_byte.Length);
                Array.Copy(binContent, a_offset, a_byte, 0, a_byte.Length);
                Array.Copy(binContent, b_offset, b_byte, 0, b_byte.Length);
                Array.Copy(binContent, c_offset, c_byte, 0, c_byte.Length);
                Array.Copy(binContent, d_offset, d_byte, 0, d_byte.Length);

                if (max_length_q < question_length) max_length_q = question_length;
                if (max_length_a < a_length) max_length_a = a_length;
                if (max_length_b < b_length) max_length_b = b_length;
                if (max_length_c < c_length) max_length_c = c_length;
                if (max_length_d < d_length) max_length_d = d_length;

                string quest_str = Encoding.Unicode.GetString(question_byte);
                string a_str = Encoding.Unicode.GetString(a_byte);
                string b_str = Encoding.Unicode.GetString(b_byte);
                string c_str = Encoding.Unicode.GetString(c_byte);
                string d_str = Encoding.Unicode.GetString(d_byte);
                
                ListQuestions.Add(new f3pStructure(quest_str, a_str, b_str, c_str, d_str));

                if (offset >= begining) break;
            }

            dataGridView1.RowCount = ListQuestions.Count;
            dataGridView1.ColumnCount = 5;
            dataGridView1.Columns[0].HeaderText = "Question";
            dataGridView1.Columns[0].Width = max_length_q * 3;
            dataGridView1.Columns[1].HeaderText = "A";
            dataGridView1.Columns[1].Width = max_length_a * 3;
            dataGridView1.Columns[2].HeaderText = "B";
            dataGridView1.Columns[2].Width = max_length_b * 3;
            dataGridView1.Columns[3].HeaderText = "C";
            dataGridView1.Columns[3].Width = max_length_c * 3;
            dataGridView1.Columns[4].HeaderText = "D";
            dataGridView1.Columns[4].Width = max_length_d * 3;

            for (int j = 0; j < ListQuestions.Count; j++)
            {
                dataGridView1.Rows[j].HeaderCell.Value = (j + 1).ToString();
                if (Form1.settings.NonUnicodeChecked)
                {
                    ListQuestions[j].question = Methods.ConvertFromLatin(ListQuestions[j].question);
                    ListQuestions[j].a_answer = Methods.ConvertFromLatin(ListQuestions[j].a_answer);
                    ListQuestions[j].b_answer = Methods.ConvertFromLatin(ListQuestions[j].b_answer);
                    ListQuestions[j].c_answer = Methods.ConvertFromLatin(ListQuestions[j].c_answer);
                    ListQuestions[j].d_answer = Methods.ConvertFromLatin(ListQuestions[j].d_answer);
                }

                dataGridView1[0, j].Value = ListQuestions[j].question;
                dataGridView1[1, j].Value = ListQuestions[j].a_answer;
                dataGridView1[2, j].Value = ListQuestions[j].b_answer;
                dataGridView1[3, j].Value = ListQuestions[j].c_answer;
                dataGridView1[4, j].Value = ListQuestions[j].d_answer;
            }
        }

        public void SaveMQP(byte[] Content, string output)
        {
            int counter = 0;

            MemoryStream index_stream = new MemoryStream(); //Копирование индексов
            MemoryStream ms = new MemoryStream(); //Запись новых строк
            MemoryStream table_stream = new MemoryStream(); //Запись таблицы строк

            for (int i = 0; i < BlockList.Count; i++)
            {
                if (BlockList[i].type == "XDNI")
                {
                    byte[] bin_block = new byte[BlockList[i].block_length];
                    Array.Copy(Content, BlockList[i].block_offset, bin_block, 0, bin_block.Length);
                    index_stream.Write(bin_block, 0, bin_block.Length);
                }

                if (BlockList[i].type == "NTSQ" && BlockList[i].counter == counter)
                {
                    byte[] table = new byte[BlockList[i].block_length];
                    Array.Copy(Content, BlockList[i].block_offset, table, 0, BlockList[i].block_length);

                    List<uint> block_length = new List<uint>(); //Длина блока с вопросом и вариантами ответов.
                    List<uint> q_offset = new List<uint>();
                    List<uint> a_var = new List<uint>();
                    List<uint> b_var = new List<uint>();
                    List<uint> c_var = new List<uint>();
                    List<uint> d_var = new List<uint>();

                    List<mqpStructure> temp = new List<mqpStructure>();

                    for (int k = 0; k < dataGridView1.RowCount; k++)
                    {
                        if (Convert.ToInt32(dataGridView1[5, k].Value) == counter)
                        {
                            temp.Add(new mqpStructure(dataGridView1[0, k].Value.ToString(), dataGridView1[1, k].Value.ToString(), dataGridView1[2, k].Value.ToString(),
                                dataGridView1[3, k].Value.ToString(), dataGridView1[4, k].Value.ToString(), counter));
                        }
                    }

                    uint text_offset = 0;

                    for (int l = 0; l < temp.Count; l++)
                    {
                        temp[l].question = Methods.ConvertToLatin(temp[l].question) + "\0";
                        temp[l].a_answer = Methods.ConvertToLatin(temp[l].a_answer) + "\0";
                        temp[l].b_answer = Methods.ConvertToLatin(temp[l].b_answer) + "\0";
                        temp[l].c_answer = Methods.ConvertToLatin(temp[l].c_answer) + "\0";
                        temp[l].d_answer = Methods.ConvertToLatin(temp[l].d_answer) + "\0";

                        byte[] q_length = Encoding.Unicode.GetBytes(temp[l].question);
                        ms.Write(q_length, 0, q_length.Length);
                        byte[] a_length = Encoding.Unicode.GetBytes(temp[l].a_answer);
                        ms.Write(a_length, 0, a_length.Length);
                        byte[] b_length = Encoding.Unicode.GetBytes(temp[l].b_answer);
                        ms.Write(b_length, 0, b_length.Length);
                        byte[] c_length = Encoding.Unicode.GetBytes(temp[l].c_answer);
                        ms.Write(c_length, 0, c_length.Length);
                        byte[] d_length = Encoding.Unicode.GetBytes(temp[l].d_answer);
                        ms.Write(d_length, 0, d_length.Length);

                        q_offset.Add(text_offset);
                        a_var.Add((uint)q_length.Length);
                        b_var.Add((uint)q_length.Length + (uint)a_length.Length);
                        c_var.Add((uint)q_length.Length + (uint)a_length.Length + (uint)b_length.Length);
                        d_var.Add((uint)q_length.Length + (uint)a_length.Length + (uint)b_length.Length + (uint)c_length.Length);
                        block_length.Add((uint)q_length.Length + (uint)a_length.Length + (uint)b_length.Length + (uint)c_length.Length + (uint)d_length.Length);

                        text_offset += (uint)q_length.Length + (uint)a_length.Length + (uint)b_length.Length + (uint)c_length.Length + (uint)d_length.Length;
                    }

                    //uint start_off = BlockList[0].block_offset;
                    uint block_offset = 8;

                    for (int l = 0; l < q_offset.Count; l++)
                    {
                        byte[] bin_q_offset = new byte[4];
                        byte[] bin_a_offset = new byte[4];
                        byte[] bin_b_offset = new byte[4];
                        byte[] bin_c_offset = new byte[4];
                        byte[] bin_d_offset = new byte[4];
                        byte[] bin_block_length = new byte[4];

                        bin_q_offset = BitConverter.GetBytes(q_offset[l]);
                        bin_block_length = BitConverter.GetBytes(block_length[l]);
                        bin_a_offset = BitConverter.GetBytes(a_var[l]);
                        bin_b_offset = BitConverter.GetBytes(b_var[l]);
                        bin_c_offset = BitConverter.GetBytes(c_var[l]);
                        bin_d_offset = BitConverter.GetBytes(d_var[l]);

                        Array.Copy(bin_q_offset, 0, table, block_offset, bin_q_offset.Length);
                        block_offset += 12;
                        Array.Copy(bin_a_offset, 0, table, block_offset, bin_a_offset.Length);
                        block_offset += 4;
                        Array.Copy(bin_b_offset, 0, table, block_offset, bin_b_offset.Length);
                        block_offset += 4;
                        Array.Copy(bin_c_offset, 0, table, block_offset, bin_c_offset.Length);
                        block_offset += 4;
                        Array.Copy(bin_d_offset, 0, table, block_offset, bin_d_offset.Length);
                        block_offset += 8;

                        uint next_offset = block_length[l];//q_offset[l + 1];
                        byte[] bin_next = new byte[4];
                        bin_next = BitConverter.GetBytes(next_offset);
                        Array.Copy(bin_next, 0, table, block_offset - 28, bin_next.Length);

                       /* if (l + 1 < q_offset.Count)
                        {
                            uint next_offset = block_length[l + 1];//q_offset[l + 1];
                            byte[] bin_next = new byte[4];
                            bin_next = BitConverter.GetBytes(next_offset);
                            Array.Copy(bin_next, 0, table, block_offset - 28, bin_next.Length);
                        }
                        else if (l + 1 == q_offset.Count)
                        {
                            uint next_offset = block_length[l];//q_offset[l + 1];
                            byte[] bin_next = new byte[4];
                            bin_next = BitConverter.GetBytes(next_offset);
                            Array.Copy(bin_next, 0, table, block_offset - 28, bin_next.Length);
                        }*/
                        //bin_start_off = BitConverter.GetBytes(start_off);

                    }

                    table_stream.Write(table, 0, table.Length);

                    q_offset.Clear();
                    block_length.Clear();
                    a_var.Clear();
                    b_var.Clear();
                    c_var.Clear();
                    d_var.Clear();
                    temp.Clear();
                    counter++;
                }
            }

            byte[] table_blocks = table_stream.ToArray();
            byte[] index_blocks = index_stream.ToArray();
            table_stream.Close();
            index_stream.Close();

            byte[] bin_length_of_begining = new byte[4];
            Array.Copy(Content, 28, bin_length_of_begining, 0, 4);


            byte[] begining = new byte[BitConverter.ToInt32(bin_length_of_begining, 0) + 12 + (12 * 15 * 3)];
            Array.Copy(Content, 0, begining, 0, begining.Length);

            MemoryStream junk_stream = new MemoryStream();

            uint blocks_offsets = (uint)begining.Length;
            int length = 0;
            counter = 0;


            for (int i = 0; i < BlockList.Count; i++)
            {
                for (int j = 0; j < dataGridView1.RowCount; j++)
                {
                    if (BlockList[i].type == "GRTS" && BlockList[i].counter == Convert.ToInt32(dataGridView1[5, j].Value))
                    {
                        string q, a, b, c, d;
                        q = Methods.ConvertToLatin(dataGridView1[0, j].Value.ToString()) + "\0";
                        a = Methods.ConvertToLatin(dataGridView1[1, j].Value.ToString()) + "\0";
                        b = Methods.ConvertToLatin(dataGridView1[2, j].Value.ToString()) + "\0";
                        c = Methods.ConvertToLatin(dataGridView1[3, j].Value.ToString()) + "\0";
                        d = Methods.ConvertToLatin(dataGridView1[4, j].Value.ToString()) + "\0";

                        byte[] bin_q = Encoding.Unicode.GetBytes(q);
                        byte[] bin_a = Encoding.Unicode.GetBytes(a);
                        byte[] bin_b = Encoding.Unicode.GetBytes(b);
                        byte[] bin_c = Encoding.Unicode.GetBytes(c);
                        byte[] bin_d = Encoding.Unicode.GetBytes(d);

                        length += bin_q.Length + bin_a.Length + bin_b.Length + bin_c.Length + bin_d.Length;
                        BlockList[i].block_length = length;

                        junk_stream.Write(bin_q, 0, bin_q.Length);
                        junk_stream.Write(bin_a, 0, bin_a.Length);
                        junk_stream.Write(bin_b, 0, bin_b.Length);
                        junk_stream.Write(bin_c, 0, bin_c.Length);
                        junk_stream.Write(bin_d, 0, bin_d.Length);
                    }
                    else length = 0;
                    /*if (counter == Convert.ToInt32(dataGridView1[5, j].Value)) blocks_lenghts += bin_q.Length + bin_a.Length + bin_b.Length + bin_c.Length + bin_d.Length;
                    else
                    {
                        blocks_offsets += (uint)blocks_lenghts;
                        BlockList[sas].block_length = blocks_lenghts;
                        result += "Total: " + blocks_lenghts + "\r\n----------------------------------------------------------------------------------\r\n";
                        blocks_lenghts = 0;
                        sas++;
                        BlockList[sas].block_offset = blocks_offsets;
                        counter++;
                    }*/

                }
            }

            blocks_offsets = (uint)begining.Length;

            for (int i = 0; i < BlockList.Count; i++)
            {
                BlockList[i].block_offset = blocks_offsets;
                blocks_offsets += (uint)BlockList[i].block_length;
            }

            byte[] strings_block = junk_stream.ToArray();
            junk_stream.Close();

            blocks_offsets = (uint)strings_block.Length + (uint)begining.Length;

            counter = 0;
            for (int i = 0; i < BlockList.Count; i++)
            {
                if ((BlockList[i].counter == counter) && (BlockList[i].type == "XDNI"))
                {
                    BlockList[i].block_offset = blocks_offsets;
                    blocks_offsets += (uint)BlockList[i].block_length;
                    counter++;
                }
            }

            counter = 0;

            for (int j = 0; j < BlockList.Count; j++)
            {
                if ((BlockList[j].counter == counter) && (BlockList[j].type == "NTSQ"))
                {
                    BlockList[j].block_offset = blocks_offsets;
                    blocks_offsets += (uint)BlockList[j].block_length;
                    counter++;
                }
            }


            uint offset = (uint)begining.Length - (12 * 15 * 3) + 4;

                for (int l = 0; l < BlockList.Count; l++)
                {
                    byte[] bin_offset = new byte[4];
                    byte[] bin_length = new byte[4];

                    bin_offset = BitConverter.GetBytes(BlockList[l].block_offset);
                    bin_length = BitConverter.GetBytes(BlockList[l].block_length);

                    Array.Copy(bin_offset, 0, begining, offset, bin_offset.Length);
                    offset += 4;
                    Array.Copy(bin_length, 0, begining, offset, bin_length.Length);
                    offset += 8;
                }

                if (File.Exists(output)) File.Delete(output);
            FileStream fs = new FileStream(output, FileMode.CreateNew);
            fs.Write(begining, 0, begining.Length);
            fs.Write(strings_block, 0, strings_block.Length);
            fs.Write(index_blocks, 0, index_blocks.Length);
            fs.Write(table_blocks, 0, table_blocks.Length);
            fs.Close();
        }

        public void ReadMQP(byte[] binContent)
        {
            int count_blocks;
            uint offset;

            byte[] bin_count = new byte[4];
            Array.Copy(binContent, 12, bin_count, 0, 4);
            count_blocks = BitConverter.ToInt32(bin_count, 0);

            byte[] bin_table_offset = new byte[4];
            Array.Copy(binContent, 28, bin_table_offset, 0, 4);

            offset = BitConverter.ToUInt32(bin_table_offset, 0) + 12;

            for (int j = 0; j < 3; j++) //Цикл для копирования 3 параметров: GRTS, XDNI и NTSQ
            {
                for (int i = 0; i < count_blocks; i++)
                {
                    string type;
                    byte[] bin_type = new byte[4];
                    uint block_offset;
                    byte[] bin_block_offset = new byte[4];
                    int block_length;
                    byte[] bin_block_length = new byte[4];
                    
                    Array.Copy(binContent, offset, bin_type, 0, 4);
                    type = Encoding.ASCII.GetString(bin_type);
                    offset += 4;

                    Array.Copy(binContent, offset, bin_block_offset, 0, 4);
                    block_offset = BitConverter.ToUInt32(bin_block_offset, 0);
                    offset += 4;

                    Array.Copy(binContent, offset, bin_block_length, 0, 4);
                    block_length = BitConverter.ToInt32(bin_block_length, 0);
                    offset += 4;

                    BlockList.Add(new mqpBlocks(type, i, block_offset, block_length));
                }
            }


            int s_num = 0; //Номер вопроса

            List<string> strings = new List<string>();

            for (int k = 0; k < BlockList.Count; k++)
            {
                //result += BlockList[k].type + "\t" + BlockList[k].counter.ToString() + "\t" + BlockList[k].block_offset.ToString() + "\t" + BlockList[k].block_length.ToString() + "\r\n";
                if ((BlockList[k].counter == s_num) && (BlockList[k].type == "GRTS"))
                {
                    byte[] bin_test = new byte[BlockList[k].block_length];
                    Array.Copy(binContent, BlockList[k].block_offset, bin_test, 0, BlockList[k].block_length);
                    string test = Encoding.Unicode.GetString(bin_test);
                    string[] multi_test = test.Split('\0');

                    for (int l = 0; l < multi_test.Length; l += 5)
                    {
                        if (l + 4 < multi_test.Length)
                        {
                            string question = multi_test[l];
                            string a_answer = multi_test[l + 1];
                            string b_answer = multi_test[l + 2];
                            string c_answer = multi_test[l + 3];
                            string d_answer = multi_test[l + 4];
                            
                            CommonListQuestions.Add(new mqpStructure(question, a_answer, b_answer, c_answer, d_answer, s_num));
                        }
                    }

                    s_num++;
                }

                //if ((BlockList[k].counter == i_num) && (BlockList[k].type == "NTSQ"))
                //{

                  //  i_num++;
                //}
            }

            dataGridView1.RowCount = CommonListQuestions.Count;
            dataGridView1.ColumnCount = 6;
            dataGridView1.Columns[0].HeaderText = "Question";
            dataGridView1.Columns[0].Width = 460;//max_length_q * 3;
            dataGridView1.Columns[1].HeaderText = "A";
            dataGridView1.Columns[1].Width = 128;//max_length_a * 3;
            dataGridView1.Columns[2].HeaderText = "B";
            dataGridView1.Columns[2].Width = 128;//max_length_b * 3;
            dataGridView1.Columns[3].HeaderText = "C";
            dataGridView1.Columns[3].Width = 128;//max_length_c * 3;
            dataGridView1.Columns[4].HeaderText = "D";
            dataGridView1.Columns[4].Width = 128; //max_length_d * 3;
            dataGridView1.Columns[5].HeaderText = "Number of question";
            dataGridView1.Columns[5].Width = 64; //max_length_d * 3;

            for (int m = 0; m < dataGridView1.RowCount; m++)
            {
                if (Form1.settings.NonUnicodeChecked)
                {
                    CommonListQuestions[m].question = Methods.ConvertFromLatin(CommonListQuestions[m].question);
                    CommonListQuestions[m].a_answer = Methods.ConvertFromLatin(CommonListQuestions[m].a_answer);
                    CommonListQuestions[m].b_answer = Methods.ConvertFromLatin(CommonListQuestions[m].b_answer);
                    CommonListQuestions[m].c_answer = Methods.ConvertFromLatin(CommonListQuestions[m].c_answer);
                    CommonListQuestions[m].d_answer = Methods.ConvertFromLatin(CommonListQuestions[m].d_answer);
                }

                dataGridView1.Rows[m].HeaderCell.Value = (m + 1).ToString();
                dataGridView1[0, m].Value = CommonListQuestions[m].question;
                dataGridView1[1, m].Value = CommonListQuestions[m].a_answer;
                dataGridView1[2, m].Value = CommonListQuestions[m].b_answer;
                dataGridView1[3, m].Value = CommonListQuestions[m].c_answer;
                dataGridView1[4, m].Value = CommonListQuestions[m].d_answer;
                dataGridView1[5, m].Value = CommonListQuestions[m].block_num;
            }

            //MessageBox.Show(result);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Questions of selection round | *.f3p| Questions | *.mqp";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                FullName = ofd.FileName;

                if (FullName != "" && File.Exists(FullName))
                {
                    FileStream fs = new FileStream(FullName, FileMode.Open);
                    binContent = Methods.ReadFull(fs);
                    fs.Close();
                    GetExtension = ofd.FileName.Remove(0, FullName.Length - 4);

                    switch (GetExtension)
                    {
                        case ".f3p":
                            ListQuestions.Clear();
                            Readf3pFiles(binContent);
                            break;
                        case ".mqp":
                            BlockList.Clear();
                            CommonListQuestions.Clear();
                            ReadMQP(binContent);
                            break;
                    }

                    actionsToolStripMenuItem.Enabled = true;
                    contextMenuStrip1.Enabled = true;
                }
            }
        }

        private void importQuestionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text file (*.txt) | *.txt";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string[] getStrings = File.ReadAllLines(ofd.FileName);
                //dataGridView1.RowCount = getStrings.Length / 2;

                if (dataGridView1.RowCount == (getStrings.Length / 2))
                {
                    string[] questions = new string[getStrings.Length / 2];
                    string[] answers = new string[getStrings.Length / 2];

                    int d = 0;
                    int k = 0;

                    for (int i = 0; i < getStrings.Length; i++)
                    {
                        if (getStrings[i].IndexOf('\\') > 0)
                        {
                            answers[d] = getStrings[i];
                            d++;
                        }
                        else
                        {
                            questions[k] = getStrings[i];
                            k++;
                        }

                        //if (k >= getStrings.Length) break;
                    }

                    int question_length = 0;
                    int a_length = 0;
                    int b_length = 0;
                    int c_length = 0;
                    int d_length = 0;

                    for (int j = 0; j < dataGridView1.RowCount; j++)
                    {
                        dataGridView1[0, j].Value = questions[j];
                        string[] variants = answers[j].Split('\\');

                        if (question_length < questions[j].Length) question_length = questions[j].Length;
                        if (a_length < variants[0].Length) a_length = variants[0].Length;
                        if (b_length < variants[1].Length) b_length = variants[1].Length;
                        if (c_length < variants[2].Length) c_length = variants[2].Length;
                        if (d_length < variants[3].Length) d_length = variants[3].Length;

                        dataGridView1[1, j].Value = variants[0];
                        dataGridView1[2, j].Value = variants[1];
                        dataGridView1[3, j].Value = variants[2];
                        dataGridView1[4, j].Value = variants[3];
                    }

                    dataGridView1.Columns[0].Width = question_length * 5;
                    dataGridView1.Columns[1].Width = a_length * 5;
                    dataGridView1.Columns[2].Width = b_length * 5;
                    dataGridView1.Columns[3].Width = c_length * 5;
                    dataGridView1.Columns[4].Width = d_length * 5;
                }
            }
        }



        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (File.Exists(FullName))
            {
                if (FullName.IndexOf(".mqp") > 0) SaveMQP(binContent, FullName);
                else Savef3pFiles(binContent, FullName);
            }
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            switch (GetExtension)
            {
                case ".f3p":
                    sfd.Filter = "Questions of selection round | *.f3p";
                    break;
                case ".mqp":
                    sfd.Filter = "Questions | *.mqp";
                    break;
            }

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                switch (GetExtension)
                {
                    case ".f3p":
                        Savef3pFiles(binContent, sfd.FileName);
                        break;
                    case ".mqp":
                        SaveMQP(binContent, sfd.FileName);
                        break;
                }
            }
        }

        private void exportQuestionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = "Text files (*.txt) | *.txt";
            sfd.Title = "Save questions";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string str = null;

                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    str += dataGridView1[0, i].Value.ToString() + "\r\n" + dataGridView1[1, i].Value.ToString()
                        + "\\" + dataGridView1[2, i].Value.ToString() + "\\" + dataGridView1[3, i].Value.ToString()
                        + "\\" + dataGridView1[4, i].Value.ToString() + "\r\n";
                }

                StreamWriter sw = new StreamWriter(sfd.FileName);
                sw.Write(str);
                sw.Close();

                str = null;
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new About().ShowDialog();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new SettingsForm().ShowDialog();
        }

        private void exportTextsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exportQuestionsToolStripMenuItem_Click(sender, e);
        }

        private void importTextsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            importQuestionsToolStripMenuItem_Click(sender, e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + "config.xml"))
            {
                string xmlPath = AppDomain.CurrentDomain.BaseDirectory + "\\config.xml";
                XmlReader reader = new XmlTextReader(xmlPath);
                XmlSerializer settingsDeserializer = new System.Xml.Serialization.XmlSerializer(typeof(Settings));
                settings = (Settings)settingsDeserializer.Deserialize(reader);
                reader.Close();
            }

            actionsToolStripMenuItem.Enabled = false;
            contextMenuStrip1.Enabled = false;
        }
    }
}
