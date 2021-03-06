﻿using Microsoft.Win32;
using System.Windows;
using System.Data;
using DLPD.Spatial;
using System.Threading;
using DLPD.Delegate;
using DLPD.Common;
using DLPD.Config;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Diagnostics;

namespace DLPD {
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : Window {
        private bool IMPORTED = false;
        private DataTable baseData;
        private Thread threadImport;
        public Main() {
            InitializeComponent();
            this.Loaded += Main_Loaded;
        }

        private void Main_Loaded(object sender, RoutedEventArgs e) {

            Constant.ZRZLFSheet = ConfigHelper.ZRZLFMarkRead();
            //throw new System.NotImplementedException();
        }

        private void btnimport_Click(object sender, RoutedEventArgs e) {
            if (!IMPORTED) {
                string filename = openFile();
                if (filename.Equals("openfaild")) {
                    MessageBox.Show("文件打开失败");
                } else {
                    readData(filename);
                }
            }
        }
        /// <summary>
        /// 新建线程读取底图文件
        /// </summary>
        /// <param name="filename"></param>
        private void readData(string filename) {
            threadImport = new Thread(() => {
                DelegateLabel lbl = new DelegateLabel(lblProgess);
                lbl.setVisibility(Visibility.Visible);
                lbl.setContent("正在加载底图数据");
                //dataGrid.ItemStringFormat = "正在加载底图数据";
                baseData = DataOperator.DataRead(filename);
                lbl.setContent("");
                lbl.setVisibility(Visibility.Hidden);
                //lblProgess.Visibility = Visibility.Hidden;
                DelegateDataGrid datagrid = new DelegateDataGrid(dtGrid);
                datagrid.setData(baseData);
                datagrid.setVisibility(Visibility.Visible);
                //dataGrid.ItemsSource = baseData.DefaultView;
                //dataGrid.Visibility = Visibility.Visible;
                MessageBox.Show("导入完成");
            });
            threadImport.Start();
        }



        /// <summary>
        /// 打开文件
        /// </summary>
        /// <returns>文件路径</returns>
        private string openFile() {
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = "shp文件(*.shp)|*.shp|所有文件(*.*)|*.*";

            if (op.ShowDialog() == true) {
                return op.FileName;
            } else {
                return "openfaild";
            }
        }

        private void btnCalculate_Click(object sender, RoutedEventArgs e) {

            if (IMPORTED) {
                MessageBox.Show("请先导入底图数据");
            } else {
                multiCalcuate();

                //Stopwatch st = new Stopwatch();
                //st.Start();
                //threadImport = new Thread(() => {
                //    DataOperator.MaxLevelCalculate(baseData);
                //    DelegateDataGrid datagrid = new DelegateDataGrid(dtGrid);
                //    datagrid.setData(baseData);
                //    st.Stop();
                //    string message = String.Format("计算完毕,用时{0}", st.Elapsed.ToString());
                //    MessageBox.Show(message);
                //});
                //threadImport.Start();

            }
        }
        /// <summary>
        /// 多线程计算
        /// </summary>
        private void multiCalcuate() {
            Stopwatch st = new Stopwatch();
            st.Start();
            #region 多线程处理
            pgbCalculate.Visibility = Visibility.Visible;

            int count = baseData.Rows.Count;
            pgbCalculate.Maximum = count;
            int length = count / 10;
            Hashtable hash = new Hashtable();
            //List<int> list = new List<int>();
            int i = 0;
            for (; i < 10; i++) {
                //list.Add(i * length);
                hash.Add(i * length, (i + 1) * length);
            }
            hash.Add(i * length, count);
            //list.Add(count);
            //int[] counts = new int[count];
            int sum = 0;
            //for (int i = 0; i < 9; i++) {
            foreach (int j in hash.Keys) {
                
                Thread thread = new Thread(() => {

                    for (int index = j; index < Int32.Parse(hash[j].ToString()); index++) {
                        DataRow row = baseData.Rows[index];
                        lock (baseData.Rows.SyncRoot) {
                            DataOperator.MaxLevelMultiCalculate(row);
                            sum++;
                            if (sum == count) {
                                //for (int start = 0; start < count; start++) {
                                //    DataOperator.DataWrite(baseData, start, counts[start]);
                                //    sum++;
                                //}
                                //if (sum == 2 * count) {
                                st.Stop();
                                string message = String.Format("计算完毕,用时{0}", st.Elapsed.ToString());
                                MessageBox.Show(message);
                                //}
                                //st.Stop();
                                //string message = String.Format("计算完毕,用时{0}", st.Elapsed.ToString());
                                //MessageBox.Show(message);

                            }
                        }
                        //double value = sum / count;
                        DelegateProgressBar progressbar = new DelegateProgressBar(pgbCalculate);
                        progressbar.setValue(sum);
                    }

                });
                thread.IsBackground = true;
                thread.Start();
                //Thread.Sleep(1);
            }
            #endregion

        }

        private void threadPoolCalcuate() {
            Stopwatch st = new Stopwatch();
            st.Start();
            ThreadPool.SetMaxThreads(100, 100);
            pgbCalculate.Visibility = Visibility.Visible;

            int count = baseData.Rows.Count;
            pgbCalculate.Maximum = count;
            int length = count / 10;
            Hashtable hash = new Hashtable();
            List<int> list = new List<int>();
            int i = 0;
            for (; i < 10; i++) {
                list.Add(i * length);
                hash.Add(i * length, (i + 1) * length);
            }
            hash.Add(i * length, count);
            list.Add(count);
            double sum = 0;
            foreach (int j in hash.Keys) {
                for (int index = j; index < Int32.Parse(hash[j].ToString()); index++) {

                    DataRow row = baseData.Rows[index];
                    lock (baseData.Rows.SyncRoot) {
                        DataOperator.MaxLevelMultiCalculate(row);
                        sum++;
                        if (sum == count) {
                            st.Stop();
                            string message = String.Format("计算完毕,用时{0}", st.Elapsed.ToString());
                            MessageBox.Show(message);

                        }
                    }
                    //double value = sum / count;
                    DelegateProgressBar progressbar = new DelegateProgressBar(pgbCalculate);
                    progressbar.setValue(sum);
                }

            }

        }

        private void dtGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            
        }

        private void binImportNew_Click(object sender, RoutedEventArgs e) {
            if (!IMPORTED) {
                string filename = openFile();
                if (filename.Equals("openfaild")) {
                    MessageBox.Show("文件打开失败");
                } else {
                    readData(filename);
                }
            }
        }
    }
}