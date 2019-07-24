using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;
using System.Timers;
using System.IO;
using System.Drawing.Imaging;
using System.Web.UI.WebControls;

namespace videoplay
{
    public partial class Play : Form
    {

        [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll ")]
        private static extern bool BitBlt(
        IntPtr hdcDest, // handle to destination DC 
        int nXDest, // x-coord of destination upper-left corner 
        int nYDest, // y-coord of destination upper-left corner 
        int nWidth, // width of destination rectangle 
        int nHeight, // height of destination rectangle 
        IntPtr hdcSrc, // handle to source DC 
        int nXSrc, // x-coordinate of source upper-left corner 
        int nYSrc, // y-coordinate of source upper-left corner 
        System.Int32 dwRop // raster operation code 
        );


        String fPath;               //文件夹路径
        String fName;               //文件名
        string[,] smap = new string[10, 2];

        Boolean textBoxHasText = false;
        Boolean isFullOrNot = false;
        Boolean muteOrNot = false;

        private FullScreenHelper fullScreenH ;

        private VlcPlayer vlc_player_;

        private bool is_playinig_;

        private int listBoxPlayIndex;

        private static object LockObject = new Object();

        SqlService ss = new SqlService();

        private static System.Timers.Timer CheckUpdatetimer = new System.Timers.Timer();            // 定义数据检查Timer

        private static int CheckUpDateLock = 0;                 // 检查更新锁
        public Play()
        {
            InitializeComponent();
            
            //到达时间的时候执行事件；
            string pluginPath = System.Environment.CurrentDirectory + "\\plugins\\";
            vlc_player_ = new VlcPlayer(pluginPath);
            IntPtr render_wnd = this.panel1.Handle;
            vlc_player_.SetRenderWindow((int)render_wnd);
            tbVideoTime.Text = "00:00:00/00:00:00";
            is_playinig_ = false;
            comboBox1.SelectedIndex = 4;
            ss.chooseSqlService();
            updateListBox2();
            fullScreenH = FullScreenHelper.createInstance(panel1);

        }
        


        //设定数据检查Timer参数       
         internal  void GetTimerStart()
         {
             // 循环间隔时间(10分钟)
             CheckUpdatetimer.Interval = 1000;
             // 允许Timer执行
             CheckUpdatetimer.Enabled = true;
             // 定义回调
             CheckUpdatetimer.Elapsed += new ElapsedEventHandler(CheckUpdatetimer_Elapsed);
             // 定义多次循环
             CheckUpdatetimer.AutoReset = true;
         }

         private void CheckUpdatetimer_Elapsed(object sender, ElapsedEventArgs e)
         {
             // 加锁检查更新锁
             lock (LockObject)
             {
                 if (CheckUpDateLock == 0) CheckUpDateLock = 1;
                 else return;
             }
             //More code goes here.
             //具体实现功能的方法
             MethodInvoker methodInvoker = new MethodInvoker(Checktimer);
             this.Invoke(methodInvoker);
             // 解锁更新检查锁
             lock (LockObject)
             {
                 CheckUpDateLock = 0;
             }
         }

         private void Checktimer()
         {
             
             if (is_playinig_)
             {
                 if (trackBarPosition.Value == trackBarPosition.Maximum)
                 {
                     //vlc_player_.Stop();
                     //CheckUpdatetimer.Stop();
                     //Pause();
                     //trackBarPosition.Value = trackBarPosition.Minimum;
                     //tbVideoTime.Text = "00:00:00/00:00:00";
                     //is_playinig_ = false;
                     if (listBox1.SelectedIndex < (listBox1.Items.Count - 1))
                     {
                         listBox1.SelectedIndex = ++listBoxPlayIndex;
                         selectIndexChanged();
                         updateListBox2();

                         //trackBarPosition.Value = trackBarPosition.Minimum;
                     }
                     else
                     {
                         vlc_player_.Stop();
                         CheckUpdatetimer.Stop();
                         Pause();
                         trackBarPosition.Value = trackBarPosition.Minimum;
                         tbVideoTime.Text = "00:00:00/00:00:00";
                     }
                 }
                 else
                 {
                     trackBarPosition.Value = trackBarPosition.Value + 1;
                     tbVideoTime.Text = string.Format("{0}/{1}",
                     GetTimeString(trackBarPosition.Value),
                         GetTimeString(trackBarPosition.Maximum));
                 }
             }
         }
        private void TrackVlaue()
        {
            trackBarPosition.SetRange(0, (int)vlc_player_.Duration());
            trackBarPosition.Value = 0;
            CheckUpdatetimer.Start(); 
        }
       
        //关闭视频
        private void CloseVideo()
        {
            if (is_playinig_)
            {
                vlc_player_.Stop();
                trackBarPosition.Value = 0;
                CheckUpdatetimer.Close();
                tbVideoTime.Text = "00:00:00/00:00:00";
                is_playinig_ = false;
            }
        }
        //暂停视频
        private void PauseVideo()
        {
            if (is_playinig_)
            {
                vlc_player_.Pause();
                //trackBar1.Value = 0;
                CheckUpdatetimer.Enabled=false;
                //is_playinig_ = false;
            }
        }
        //播放视频
        private void VidepoPlay(string filepath)
        {
            try
            {
                is_playinig_ = true;
                vlc_player_.PlayFile(filepath);
                
                MethodInvoker methodInvoker = new MethodInvoker(TrackVlaue);
                this.Invoke(methodInvoker);
            }
            catch
            { 
            }
        }

        private void SetRate(float rate)
        {
            if (is_playinig_)
            {
                vlc_player_.SetRate(rate);
                CheckUpdatetimer.Interval = (int)(1000 / rate);
            }
        }
        //继续播放视频
        private void ContinueVideo()
        {
            if (is_playinig_) 
            {                
                vlc_player_.Pause();
                CheckUpdatetimer.Enabled = true;
            }
        }
        private void RestartVideo()
        {
            if (is_playinig_)
            { 
                
            }
        }
        //快进or快退
        private void GoForwardVideo(double time)
        {
            if (is_playinig_)
            {
                if (vlc_player_.GetPlayTime() + time >= trackBarPosition.Minimum && vlc_player_.GetPlayTime() + time <= trackBarPosition.Maximum)
                {
                    vlc_player_.SetPlayTime(vlc_player_.GetPlayTime() + time);
                    trackBarPosition.Value = (int)vlc_player_.GetPlayTime();
                }
                else if (vlc_player_.GetPlayTime() + time < trackBarPosition.Minimum)
                {
                    vlc_player_.SetPlayTime(0);
                    trackBarPosition.Value = (int)vlc_player_.GetPlayTime();
                }
                else
                {
                    if (listBox1.SelectedIndex < (listBox1.Items.Count - 1))
                    {
                        listBox1.SelectedIndex++;
                        selectIndexChanged();
                    }
                    else
                    {
                        MessageBox.Show("已到达播放列表底端", "注意", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        is_playinig_ = false;
                    }
                }
            }
        }
        //声音
        private void SetVolumeVideo(int value)
        {
            if (is_playinig_)
            {
                vlc_player_.SetVolume(value);
            }
        }

        
        private string GetTimeString(int val)
        {
            int hour = val / 3600;
            val %= 3600;
            int minute = val / 60;
            int second = val % 60;
            return string.Format("{0:00}:{1:00}:{2:00}", hour, minute, second);
        }

        private void Play_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseVideo();
        }

        private void Play_Load(object sender, EventArgs e)
        {      
            GetTimerStart();
        }

        private void 文件FToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        //打开文件按钮点击事件
        private void 打开文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
              openFile();
              comboBox1_SelectionChangeCommitted(null, null);
        }

        private void panel3_MouseEnter(object sender, EventArgs e)
        {
           
        }

        private void panel3_MouseLeave(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (fPath == null | is_playinig_==false)
            {
                openFile();//
            }
            else
            {
                Start();
                ContinueVideo();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Pause();
            PauseVideo();
        }
        //打开文件函数
        private void openFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();//创建打开文件窗口实例

            openFileDialog.Filter = "Medie Files|*.mp4;*.avi;*.flv;*.wmv;*.mkv;*.mov;*.rmvb;*.asf;*.wav;*.mp2;*.mp3;*.wma|All Files|*.*";//判断多媒体格式，若符合要求则可添加
            if (System.Windows.Forms.DialogResult.OK == openFileDialog.ShowDialog())//点击确定后执行事件
            {
                if (fPath == null)//是第一次打开文件
                {
                    this.Text = openFileDialog.SafeFileName;//窗体标题改为正在播放文件名
                    fPath = System.IO.Path.GetDirectoryName(openFileDialog.FileName);//路径赋值为播放文件所在路径
                    fName = openFileDialog.SafeFileName;//文件名赋值为播放文件名
                    listBox1.Items.Add(openFileDialog.SafeFileName);//将播放文件名添加至播放列表
                    listBox1.SelectedIndex = 0;//播放列表默认选中第一个文件
                    VidepoPlay(openFileDialog.FileName);//播放打开文件
                    Start();//将播放按钮隐藏，暂停按钮显示
                    ss.insert(fPath, fName, DateTime.Now);//播放文件的路径、名称、播放时间上传至数据库
                    updateListBox2();//更新历史记录栏
                }
                else if (fPath.Equals(System.IO.Path.GetDirectoryName(openFileDialog.FileName)))//不是第一次打开文件，该次打开文件和上一次打开文件在同一个文件夹
                {
                    this.Text = openFileDialog.SafeFileName;
                    fPath = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                    fName = openFileDialog.SafeFileName;
                    if (checkFileName(openFileDialog.SafeFileName))//该次打开文件不在播放列表内
                    {
                        listBox1.Items.Add(openFileDialog.SafeFileName);
                        listBox1.SetSelected(listBox1.Items.Count-1,true);
                        listBoxPlayIndex = listBox1.Items.Count - 1;
                    }
                    else//该次打开文件在播放列表内
                    {
                        listBox1.SetSelected(getListBoxIndex(openFileDialog.SafeFileName),true);
                        listBoxPlayIndex = getListBoxIndex(openFileDialog.SafeFileName);
                    }
                    VidepoPlay(openFileDialog.FileName);
                    Start();
                    ss.insert(fPath, fName, DateTime.Now);
                    updateListBox2();
                }
                else//不是第一次打开文件，且该次打开文件与上一次打开文件不再同一个文件夹
                {
                    listBox1.Items.Clear();//清空播放列表
                    this.Text = openFileDialog.SafeFileName;
                    fPath = System.IO.Path.GetDirectoryName(openFileDialog.FileName);//更新路径为此次打开文件所在路径
                    fName = openFileDialog.SafeFileName;//更新文件名
                    listBox1.Items.Add(openFileDialog.SafeFileName);
                    listBox1.SelectedIndex = 0;
                    VidepoPlay(openFileDialog.FileName);
                    Start();
                    ss.insert(fPath, fName, DateTime.Now);
                    updateListBox2();
                }
            }
        }
        //检测同一个文件夹下是否重名
        bool checkFileName(string name)
        {
            foreach (var s in listBox1.Items)
            { 
                if(name.Trim()==s.ToString().Trim())
                {
                    return false;
                }
            }
            return true;
        }

        private int getListBoxIndex(string name)
        {
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                if (listBox1.Items[i].ToString().Trim().Equals(name.Trim()))
                {
                    return i;
                }
            }
            return -1;
        }
        //开始时按钮显隐
        private void Start() 
        {
            buttonPlay.Visible = false;
            buttonPause.Visible = true;
        }
        //暂停是按钮显隐
        private void Pause()
        {
            buttonPause.Visible = false;
            buttonPlay.Visible = true;
            
        }
        //打开文件夹按钮点击事件
        private void 打开文件夹ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(folderBrowserDialog1.ShowDialog()==System.Windows.Forms.DialogResult.OK)//点击确定，添加文件夹内文件
            {
                if(fPath != folderBrowserDialog1.SelectedPath)//若是第一次添加
                {
                    listBox1.Items.Clear();
                    fPath = folderBrowserDialog1.SelectedPath;
                    DirectoryInfo dir = new DirectoryInfo(fPath);

                    Array files = dir.GetFiles();
                    if (files.Length > 0)
                    {
                        foreach (FileInfo fileInfo in files)
                        {
                            if (   fileInfo.Extension == ".avi"
                                || fileInfo.Extension == ".mp4"
                                || fileInfo.Extension == ".mp3"//音频
                                || fileInfo.Extension == ".rmvb"
                                || fileInfo.Extension == ".wmv"
                                || fileInfo.Extension == ".wma"//音频
                                || fileInfo.Extension == ".flv"
                                || fileInfo.Extension == ".mkv"
                                || fileInfo.Extension == ".mov"
                                || fileInfo.Extension == ".asf"
                                || fileInfo.Extension == ".wav"//音频
                                || fileInfo.Extension == ".mp2"//音频
                               )
                            {
                                listBox1.Items.Add(fileInfo.Name);
                            }
                        }
                    }
                }
                else//若不是第一次添加
                {

                    DirectoryInfo dir = new DirectoryInfo(fPath);

                    Array files = dir.GetFiles();
                    if (files.Length > 0)
                    {
                        foreach (FileInfo fileInfo in files)
                        {
                            if (fileInfo.Extension == ".avi"
                                || fileInfo.Extension == ".mp4"
                                || fileInfo.Extension == ".mp3"
                                || fileInfo.Extension == ".rmvb"
                                || fileInfo.Extension == ".wmv"
                                || fileInfo.Extension == ".wma"
                                || fileInfo.Extension == ".flv"
                                || fileInfo.Extension == ".mkv"
                                || fileInfo.Extension == ".mov"
                                || fileInfo.Extension == ".asf"
                                || fileInfo.Extension == ".wav"
                                || fileInfo.Extension == ".mp2")
                            {
                                if (checkFileName(fileInfo.Name))
                                {
                                    listBox1.Items.Add(fileInfo.Name);
                                }
                            }
                        }
                    }
                }
            }
        }

        //listBox1双击执行函数
        public void selectIndexChanged()
        {
            
                fName = Convert.ToString(listBox1.SelectedItem);
                VidepoPlay(fPath + "\\" + fName);
                Start();
            
                ss.insert(fPath, fName, DateTime.Now);
        }

        //检查名称是否存在listBox2
        bool checkFileNameInListBox2(string name)
        {
            
            foreach (var s in listBox2.Items)
            {
                if (name.Trim() == s.ToString().Trim())
                {
                    return false;
                }
            }
            return true;
        }
        //listBox1双击事件
        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                selectIndexChanged();
                listBoxPlayIndex = listBox1.SelectedIndex;
                updateListBox2();
                comboBox1_SelectionChangeCommitted(null, null);
            }
           
        }

        private void button7_Click(object sender, EventArgs e)
        {
            CloseVideo();
            Pause();
            //fPath = null;
            is_playinig_ = false;
        }

        
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Space)
            {
                if (buttonPause.Visible == true)
                    this.button1_Click(null, null);
                else
                    this.button2_Click(null, null);
                return true;
            }
            if (keyData == Keys.Left)
            {
                button3_Click(null, null);
                return true;
            }
            if (keyData == Keys.Right)
            {
                button4_Click(null, null);
                return true;
            }
            if (keyData == Keys.Escape)
            {
                
                if (isFullOrNot)
                {
                    fullScreenH.FullScreen(false);
                    isFullOrNot = false;
                }
                else
                {
                    
                }
                return true;
            }
            if (keyData == Keys.Enter)
            {
                this.button8_Click(null, null);
                return true;
            }
            if (keyData == Keys.Up)
            {
                if (trackBarVolume.Value < 100)
                {
                    trackBarVolume.Value += 1;
                    SetVolumeVideo(trackBarVolume.Value);
                }
                return true;
            }
            if (keyData == Keys.Down)
            {
                if (trackBarVolume.Value > 0)
                {
                    trackBarVolume.Value -= 1;
                    SetVolumeVideo(trackBarVolume.Value);
                }
                return true;
            }
            return base.ProcessCmdKey(ref msg,keyData);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            GoForwardVideo(10);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            GoForwardVideo(-10);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < (listBox1.Items.Count - 1))
            {
                listBox1.SelectedIndex = ++listBoxPlayIndex;
                selectIndexChanged();
                updateListBox2();
            }
            else
            {
                MessageBox.Show("已到达播放列表底端", "注意", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                is_playinig_ = false;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex > 0)
            {
                listBox1.SelectedIndex--;
                selectIndexChanged();
                updateListBox2();
            }
            else
                MessageBox.Show("已到达播放列表顶端", "注意", MessageBoxButtons.OK, MessageBoxIcon.Warning);   
        }

        private void trackBar1_Scroll_1(object sender, EventArgs e)
        {
            if (is_playinig_)
            {
                vlc_player_.SetPlayTime(trackBarPosition.Value);
                trackBarPosition.Value = (int)vlc_player_.GetPlayTime();
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (isFullOrNot)
            {
                fullScreenH.FullScreen(false);
                isFullOrNot = false;
            }
            else
            {
                fullScreenH.FullScreen(true);
                isFullOrNot = true;
            }
        }

        private void textBoxSearch_Enter(object sender, EventArgs e)
        {
            if (textBoxHasText == false)
                textBoxSearch.Text = "";

            textBoxSearch.ForeColor = Color.Black;
        }

        private void textBoxSearch_Leave(object sender, EventArgs e)
        {
            if (textBoxSearch.Text == "")
            {
                textBoxSearch.Text = "[搜索框]";
                textBoxSearch.ForeColor = Color.LightGray;
                textBoxHasText = false;
            }
            else
                textBoxHasText = true;
        }

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            if (textBoxSearch.Text == ""|textBoxSearch.Text=="[搜索框]")
            {
                if (fPath == null)
                { }
                else
                {
                    listBox1.Items.Clear();
                    DirectoryInfo dir = new DirectoryInfo(fPath);

                    Array files = dir.GetFiles();
                    if (files.Length > 0)
                    {
                        foreach (FileInfo fileInfo in files)
                        {
                            if (fileInfo.Extension == ".avi"
                                || fileInfo.Extension == ".mp4"
                                || fileInfo.Extension == ".mp3"
                                || fileInfo.Extension == ".rmvb"
                                || fileInfo.Extension == ".wmv"
                                || fileInfo.Extension == ".wma"
                                || fileInfo.Extension == ".flv"
                                || fileInfo.Extension == ".mkv"
                                || fileInfo.Extension == ".mov"
                                || fileInfo.Extension == ".asf"
                                || fileInfo.Extension == ".wav"
                                || fileInfo.Extension == ".mp2")
                            {
                                listBox1.Items.Add(fileInfo.Name);
                            }
                        }
                    }
                }
            }
            else
            {
                if (listBox1.Items.Count == 0)
                {

                }
                else
                {
                    List<string> s = searchFileName(textBoxSearch.Text.Trim());
                    listBox1.Items.Clear();
                    foreach (var ss in s)
                    {
                        listBox1.Items.Add(ss);
                    }
                }
            }
        }

        private List<string> searchFileName(string fileName)
        { 
            List<string> names=new List<string>();

            foreach (var items in listBox1.Items)
            {
                if (items.ToString().Trim().Contains(fileName.Trim()))
                {
                    names.Add(items.ToString().Trim());
                }
            }
            return names;
        }

        private void panel1_DoubleClick(object sender, EventArgs e)
        {
            if (isFullOrNot)
            {
                fullScreenH.FullScreen(false);
                isFullOrNot = false;
            }
            else
            {
                fullScreenH.FullScreen(true);
                isFullOrNot = true;
            }
        }

        private void buttonVolume_Click(object sender, EventArgs e)
        {
            buttonVolume.Visible = false;
            buttonMute.Visible = true;
            SetVolumeVideo(0);
            muteOrNot = true;
        }

        private void buttonMute_Click(object sender, EventArgs e)
        {
            buttonVolume.Visible = true;
            buttonMute.Visible = false;
            SetVolumeVideo((int)((1.0 * trackBarVolume.Value / trackBarVolume.Maximum) * 100));
            muteOrNot = false;
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            if (muteOrNot)
            { }
            else
            {
                SetVolumeVideo((int)((1.0 * trackBarVolume.Value / trackBarVolume.Maximum) * 100));
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            
        }

        private void trackBarVolume_Scroll(object sender, EventArgs e)
        {

        }

        private void 停止ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button7_Click(null,null);
        }

        private void 上一个ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button6_Click(null, null);
        }

        private void 快退ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button3_Click(null, null);
        }

        private void 快进ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button4_Click(null, null);
        }

        private void 下一个ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button5_Click(null, null);
        }

        private void 暂停ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fPath != null)
            {
                if (is_playinig_ == true)
                { 
                    if (buttonPause.Visible == true)
                    {
                        PauseVideo();
                        Pause();
                    }
                    else if (buttonPlay.Visible == true)
                    {
                        ContinueVideo();
                        Start();
                    }
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
                SetRate(3);
            if (comboBox1.SelectedIndex == 1)
                SetRate(2);
            if (comboBox1.SelectedIndex == 2)
                SetRate((float)1.5);
            if (comboBox1.SelectedIndex == 3)
                SetRate((float)1.25);
            if (comboBox1.SelectedIndex == 4)
                SetRate(1);
            if (comboBox1.SelectedIndex == 5)
                SetRate((float)0.75);
            if (comboBox1.SelectedIndex == 6)
                SetRate((float)0.5);
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void buttonShot_Click(object sender, EventArgs e)
        {
            if (is_playinig_)
            {
                ScreenCapture sc = new ScreenCapture();
                System.Drawing.Image img = sc.CaptureScreen();
                sc.CaptureWindowToFile(this.panel1.Handle, "ScreenShot\\" + System.DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss-ffff") + ".bmp", ImageFormat.Png);
            }
            else
            {
                MessageBox.Show("请播放文件后再截屏", "注意", MessageBoxButtons.OK, MessageBoxIcon.Warning); 
            }
        }

        private void updateListBox2()
        {
            int count=0;
            listBox2.Items.Clear();
            foreach (var s in ss.getTen())
            {
                //listBox2.Items.Add(s);
                ListItem li = new ListItem();
                
                li.Text = s.Name;
                listBox2.Items.Add(li);
                insertmap(s, count++);
            }
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int listbox2SI = listBox2.SelectedIndex;
            string dPath = smap[listbox2SI, 0];
            string dName = smap[listbox2SI, 1];

            ss.delete(dPath,dName);
            updateListBox2();
        }

        private void 全屏ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isFullOrNot)
            {
                fullScreenH.FullScreen(false);
                isFullOrNot = false;
            }
            else
            {
                fullScreenH.FullScreen(true);
                isFullOrNot = true;
            }
        }

        private void 截图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (is_playinig_)
            {
                Graphics g1 = panel1.CreateGraphics();
                System.Drawing.Image myImage = new Bitmap(this.panel1.Width, this.panel1.Height, g1);
                Graphics g2 = Graphics.FromImage(myImage);
                IntPtr dc1 = g1.GetHdc();
                IntPtr dc2 = g2.GetHdc();
                BitBlt(dc2, 0, 0, this.panel1.Width, this.panel1.Height, dc1, 0, 0, 13369376);
                g1.ReleaseHdc(dc1);
                g2.ReleaseHdc(dc2);
                myImage.Save(System.DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss-ffff") + ".bmp", ImageFormat.Bmp);
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            SetRate(3);
            comboBox1.SelectedIndex = 0;
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            SetRate(2);
            comboBox1.SelectedIndex = 1;
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            SetRate((float)1.5);
            comboBox1.SelectedIndex = 2;
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            SetRate((float)1.25);
            comboBox1.SelectedIndex = 3;
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            SetRate(1);
            comboBox1.SelectedIndex = 4;
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            SetRate((float)0.75);
            comboBox1.SelectedIndex = 5;
        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            SetRate((float)0.5);
            comboBox1.SelectedIndex = 6;
        }

        private Boolean insertmap(RecordMember rm,int i)
        {
            bool upornot=false;
            if(rm!=null)
            {
                smap[i, 0] = rm.Path;
                smap[i, 1] = rm.Name;
            }
            return upornot;
        }

        private void listBox2_DoubleClick(object sender, EventArgs e)
        {
            int listbox2SI = listBox2.SelectedIndex;
            if (listbox2SI >= 0)
            {
                string pPath = smap[listbox2SI, 0];
                string pName = smap[listbox2SI, 1];
                VidepoPlay(pPath + "\\" + pName);
                Start();
                ss.insert(pPath, pName, DateTime.Now);
                updateListBox2();
                comboBox1_SelectionChangeCommitted(null, null);
            }
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void listBox1_MouseMove(object sender, MouseEventArgs e)
        {
            int index = listBox1.IndexFromPoint(e.Location);
            // Check if the index is valid.
            if (index != -1 && index < listBox1.Items.Count)
            {
                // Check if the ToolTip's text isn't already the one
                // we are now processing.
                if (toolTip1.GetToolTip(listBox1) != listBox1.Items[index].ToString())
                {
                    // If it isn't, then a new item needs to be
                    // displayed on the toolTip. Update it.
                    toolTip1.SetToolTip(listBox1, listBox1.Items[index].ToString());
                }
            }
        }

        private void listBox2_MouseMove(object sender, MouseEventArgs e)
        {
            int index = listBox2.IndexFromPoint(e.Location);
            // Check if the index is valid.
            if (index != -1 && index < listBox2.Items.Count)
            {
                // Check if the ToolTip's text isn't already the one
                // we are now processing.
                if (toolTip1.GetToolTip(listBox2) != listBox2.Items[index].ToString())
                {
                    // If it isn't, then a new item needs to be
                    // displayed on the toolTip. Update it.
                    toolTip1.SetToolTip(listBox2, listBox2.Items[index].ToString());
                }
            }
        }        
    }
}
