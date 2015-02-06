﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MyStockAnalyzer.Helpers;
using MyStockAnalyzer.Models;
using Microsoft.VisualBasic.FileIO;
using System.Threading;
using MyStockAnalyzer.StockSelectionAlgorithms;
using MyStockAnalyzer.StockSelectionAlgorithms.Interfaces;

namespace MyStockAnalyzer
{
    public partial class FrmMain : Form
    {
        private StockModel model = new StockModel();
        private MemoModel memoModel = new MemoModel();
        private StockHelper stockHelper = new StockHelper();
        private FrmBrowser frmBrowser;

        private List<IStockSelectionAlgorithm> stockSelectionAlgorithms = new List<IStockSelectionAlgorithm>();

        /// <summary>
        /// 下載股價資訊的執行緒
        /// </summary>
        private Thread[] threadDownloadStockPrice = new Thread[10];

        /// <summary>
        /// 存放要更新的股價資訊
        /// </summary>
        private List<StockPrice> waitedUpdateSotckPrice = new List<StockPrice>();

        public FrmMain()
        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            LogHelper.DisplayGridView = this.dgvLog;
            dtStockEnd.Value = DateTime.Now;
            dtStockBgn.Value = stockHelper.LastGetStockDate;

            lblAlgorithms.Text = String.Empty;
            stockSelectionAlgorithms = StockSelectionAlgorithmHelper.GetDefaultAlgorithms();
            setStockAlgorithmsLabel();

            refreshWarrant();

            txtMemo.Text = memoModel.GetMemo();
        }

        private void setStockAlgorithmsLabel()
        {
            lblAlgorithms.Text = String.Format("({0})", String.Join(";", stockSelectionAlgorithms.Select(x => x.Name).ToArray()));
            toolTips.SetToolTip(lblAlgorithms, lblAlgorithms.Text);
        }

        private void refreshWarrant()
        {
            dgvWarrantStock.Rows.Clear();
            dgvWarrantList.Rows.Clear();

            List<StockData> warrantTargetList = model.GetWarrantTargetStockData();
            foreach (StockData stock in warrantTargetList)
            {
                dgvWarrantStock.Rows.Add(new string[] { stock.StockId, stock.StockName });
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            StockData data = model.GetStockDataById("3086");
            List<StockPrice> result = stockHelper.GetStockRealTimePrice(new List<StockData>() { data }, DateTime.Now.Date);
        }

        private void showChartDialog(string url, string title)
        {
            frmBrowser = new FrmBrowser();
            frmBrowser.Text = title;
            frmBrowser.SetUrl(url);
            frmBrowser.Show();
        }
        #region 更新股票資訊

        private void btnUpdateStockData_Click(object sender, EventArgs e)
        {
            btnUpdateStockData.Enabled = false;

            LogHelper.SetLogMessage("更新股票資訊中...");

            // 1. 更新股票代碼
            LogHelper.SetLogMessage("下載股票代碼");
            List<StockData> stockDataList = stockHelper.GetStockDataList();

            LogHelper.SetLogMessage("更新股票代碼");
            model.UpdateStockList(stockDataList);

            // 2. 更新大盤資訊
            LogHelper.SetLogMessage("更新大盤資訊");

            // 3. 更新個股資訊
            LogHelper.SetLogMessage("更新股票價格資訊");
            updateAllStockPrice(stockDataList);

            LogHelper.SetLogMessage("完成");

            btnUpdateStockData.Enabled = true;
        }

        /// <summary>
        /// 擷取並更新股價資料庫
        /// </summary>
        /// <param name="stockDataList"></param>
        private void updateAllStockPrice(List<StockData> stockDataList)
        {
            model.DeleteStockPriceByDateRange(dtStockBgn.Value, dtStockEnd.Value);

            waitedUpdateSotckPrice.Clear();

            foreach (StockData stock in stockDataList)
            {
                do
                {
                    int workNum = getAvailableBackgroundWorkNum();
                    if (workNum != -1)
                    {
                        LogHelper.SetLogMessage(String.Format("(Thread {6}) 抓取股票 {0} {1} {2}/{3} ~ {4}/{5}資料", stock.StockId, stock.StockName,
                            dtStockBgn.Value.Year, dtStockBgn.Value.Month.ToString("00"),
                            dtStockEnd.Value.Year, dtStockEnd.Value.Month.ToString("00"),
                            (workNum + 1).ToString("00")
                            ));

                        threadDownloadStockPrice[workNum].Start(stock);
                        break;
                    }
                } while (true);
            }

            while (!isAllBackgroundWorkerDone())
            {
                Application.DoEvents();
            }
            model.UpdateStockPriceList(waitedUpdateSotckPrice);
        }

        /// <summary>
        /// 執行下載股價資訊執行緒
        /// </summary>
        /// <param name="obj"></param>
        private void threadDownloadStockPrice_Start(object obj)
        {
            if (obj is StockData)
            {
                StockData stock = obj as StockData;
                List<StockPrice> singleStockPrices = stockHelper.GetStockPriceDataList(stock, dtStockBgn.Value.Date, dtStockEnd.Value.Date);
                lock (this)
                {
                    waitedUpdateSotckPrice.AddRange(singleStockPrices);
                    //if (waitedUpdateSotckPrice.Count() >= 1000)
                    //{
                    //    model.UpdateStockPriceList(waitedUpdateSotckPrice);
                    //    waitedUpdateSotckPrice.Clear();
                    //}
                }
            }
        }

        /// <summary>
        /// 取得可欲的執行緒index
        /// </summary>
        /// <returns></returns>
        private int getAvailableBackgroundWorkNum()
        {
            for (int i = 0; i < 10; ++i)
            {
                if (threadDownloadStockPrice[i] == null || threadDownloadStockPrice[i].ThreadState == ThreadState.Unstarted || threadDownloadStockPrice[i].ThreadState == ThreadState.Stopped)
                {
                    threadDownloadStockPrice[i] = new Thread(threadDownloadStockPrice_Start);
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 確認是否所有執行緒皆已完成
        /// </summary>
        /// <returns></returns>
        private bool isAllBackgroundWorkerDone()
        {
            for (int i = 0; i < 10; ++i)
            {
                if (threadDownloadStockPrice[i].ThreadState == ThreadState.Running)
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region 選股Tab

        private void linkLblClearLog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            dgvLog.Rows.Clear();
        }

        private void btnSelection_Click(object sender, EventArgs e)
        {
            if (this.stockSelectionAlgorithms.Count == 0)
            {
                MessageBox.Show("尚未選擇任何選股演算法");
                return;
            }

            dgvSelectionResult.Rows.Clear();

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            LogHelper.SetLogMessage("開始進行選股動作..");

            // 擷取即時資料
            List<StockData> stockData = model.GetAllStockData();
            List<StockPrice> realTimeData = new List<StockPrice>();
            if (chkRealData.Checked)
            {
                LogHelper.SetLogMessage(String.Format("下載即時資料"));
                List<StockData> tmp = new List<StockData>();
                foreach (StockData data in stockData)
                {
                    tmp.Add(data);
                    if (tmp.Count >= 50)
                    {
                        realTimeData.AddRange(stockHelper.GetStockRealTimePrice(tmp, DateTime.Now.Date));
                        tmp.Clear();
                    }
                }
                if (tmp.Count() > 0)
                {
                    realTimeData.AddRange(stockHelper.GetStockRealTimePrice(tmp, DateTime.Now.Date));
                }
                tmp.Clear();
            }
           
            // 分析股票資料
            foreach (StockData data in stockData)
            {
                Dictionary<string, List<StockPrice>> stockPrice = model.GetStockPriceData(new string[] { data.StockId }, dtSelectionBgn.Value.Date.AddMonths(-12), dtSelectionEnd.Value.Date);
                foreach (KeyValuePair<string, List<StockPrice>> kvp in stockPrice)
                {
                    if (chkRealData.Checked && realTimeData.Where(x => x.StockId == kvp.Key).Count() > 0)
                    {
                        kvp.Value.AddRange(realTimeData.Where(x => x.StockId == kvp.Key));
                    }
                    List<StockChartData> chartData = StockAnalysisHelper.StockPriceDataToChart(kvp.Value);

                    foreach (IStockSelectionAlgorithm algorithm in this.stockSelectionAlgorithms)
                    {
                        // 當演算法指定只檢查權證標的時, 忽略非權證標的個股
                        if (algorithm is IStockSelectionConditionWarrantOnly)
                        {
                            if (data.WarrantTarget == null || !data.WarrantTarget.Equals("Y"))
                            {
                                continue;
                            }
                        }

                        List<StockSelectionResult> result = algorithm.GetSelectionResult(chartData, dtSelectionBgn.Value.Date, dtSelectionEnd.Value.Date.AddDays(1).AddSeconds(-1));
                        foreach (StockSelectionResult ss in result)
                        {
                            dgvSelectionResult.Rows.Add(new string[] { ss.Date.ToString("yyyy/MM/dd"), data.StockId, data.StockName, algorithm.Name, data.WarrantTarget, ss.Memo });
                        }
                    }
                }
            }
            sw.Stop();
            LogHelper.SetLogMessage(String.Format("完成選股，花費 {0} 秒", (sw.ElapsedMilliseconds / 1000).ToString("0.00")));
        }

        private void linkLblAlgorithms_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FrmAlgorithm frmAlgorithm = new FrmAlgorithm();
            frmAlgorithm.CurrentAlgorithms = this.stockSelectionAlgorithms;
            frmAlgorithm.ShowDialog();
            if (frmAlgorithm.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                this.stockSelectionAlgorithms.Clear();
                this.stockSelectionAlgorithms.AddRange(frmAlgorithm.ReturnedAlgorithms);
                setStockAlgorithmsLabel();
            }
        }

        private void btnSelectionExport_Click(object sender, EventArgs e)
        {
            if (saveCSVDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("時間,代碼,名稱,篩選方法,權證,備註");

                foreach (DataGridViewRow row in dgvSelectionResult.Rows)
                {
                    sb.AppendLine(String.Format("{0},{1},{2},{3},{4},{5}",
                        row.Cells["colSelectionDate"].Value.ToString(), row.Cells["colSelectionStockId"].Value.ToString(), row.Cells["colSelectionStockName"].Value.ToString(),
                        row.Cells["colSelectionMethod"].Value.ToString(), row.Cells["colSelectionWarrant"].Value.ToString(),
                        String.Join(",", row.Cells["colSelectionMemo"].Value.ToString().Split(new string[] { ";", ":" }, StringSplitOptions.None))));
                }

                System.IO.File.WriteAllText(saveCSVDialog.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("選股資料已匯出");
            }
        }

        private void dgvSelectionResult_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == colSelectionStockId.Index)
            {
                showChartDialog(String.Format("http://www.wantgoo.com/Stock2/Chart/?StockNo={0}", dgvSelectionResult.Rows[e.RowIndex].Cells[colSelectionStockId.Name].Value.ToString()),
                    dgvSelectionResult.Rows[e.RowIndex].Cells[colSelectionStockId.Name].Value.ToString() + "-" + dgvSelectionResult.Rows[e.RowIndex].Cells[colSelectionStockName.Name].Value.ToString());
            }
        }

        #endregion

        #region 權證Tab

        private void linkLblWarrantRefresh_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            refreshWarrant();
        }

        #endregion

        #region 備註Tab

        private void btnSaveMemo_Click(object sender, EventArgs e)
        {
            memoModel.SaveMemo(txtMemo.Text);
        }

        #endregion
    }
}