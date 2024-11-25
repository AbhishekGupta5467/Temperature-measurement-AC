using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO.Ports;
using System.Net;


namespace WindowsFormsApplication1
{  

    public partial class MainScreen : Form
    {
        String VersionNum = "1.0.10a";
        String VersionDate = "DEC 10, 2019";

        /*
         * VER 1.0.10   Dec 10, 2019    Add cold protect
         * Ver 1.0.10   Nov 18, 2019    do brightness adjustment one a second
         * a                            Working on time zone
		 * back							change brightness setting to use lux1 - Lux100
         * Ver 1.0.9    Nov 17, 2019    Add Lux to DEFAULT XML file
         * Ver 1.0.8    Nov 16, 2019    Fix WDError and RWD Error
         * Ver 1.0.7    Nov 15, 2019    Added Lux and display brightness control.
         * Ver 1.0.6    Nov 14, 2019    Changed temperture error reporting and ndisplay
         * Ver 1.0.5    Nov 11, 2019    Added tries to startprocess to allow running on PV
         *                              Changed hown temp sensor errors are reported and displayed
         * Ver 1.0.4    Nov 10, 2019    Add return serial alarm indicator to status line, either WD or RWD will show as SERIAL ERROR
         * a                            if recieving comm fail, set sensors to failed minimums
         *                              Heat set pint - turns on,  +1 turns off
             * 
         * Ver 1.0.3    Nov 6, 2019     Read WR - this is thre watchdog we sent to the Arduino, and it returns it.
         * Ver 1.0.2    Nov 5, 2019     Change Ser TX to once a second rather than 250 ms.
         * Ver 1.0.0    Nov 2, 2019     Initial Release
         * Ver 1.0.1    Nov 4, 2019     Make -99 display as a "-"
         *                              
         * 
         */


        SerialPort myport = new SerialPort("/dev/ttyAMA0", 19200, Parity.None, 8, StopBits.One);
        // SerialPort myport = new SerialPort("COM1", 19200, Parity.None, 8, StopBits.One);
        // SerialPort myport = new SerialPort("/dev/serial0", 9600, Parity.None, 8, StopBits.One);

        Boolean ShowSetupScreen = false;
        Boolean ModeButtonFront = false;
        Boolean ModeButtonRear = false;
        Boolean ModeButtonFrontOLD = false;
        Boolean ModeButtonRearOLD = false;

        int FRMode = 0;
        int FRMode_OLD = 0;
        int RRMode = 0;
        int RRMode_OLD = 0;

        // int MBSleep = 60;
 
        // int FR_SP = 65;
        // int RR_SP = 67;

        int HEATFSP = 45;
        int HEATRSP = 45;
        int ACFSP = 45;
        int ACRSP = 45;

        int HEATFSP_OLD = 45;
        int HEATRSP_OLD = 45;
        int ACFSP_OLD = 45;
        int ACRSP_OLD = 45;

        int HVAC_FR = 45;
        int HVAC_RR = 45;
        int HVAC_OAT = 45;
     
        int TempSourceFront = 1;
        int TempSourceRear = 2;
        int TempSourceOutside = 3;
        int Temp1Cnt = 0;
        int Temp2Cnt = 0;
        int Temp3Cnt = 0;

        int Lux = 10;  //  light level coming from arduino
        int LuxHI = 40;  // 
        int LuxMED = 5;  // below this, dims  
        int LuxLO = 2;  // below this, dims a lot
        int LuxBriMAX = 200;
        int LuxBriHI = 100;  // 
        int LuxBriMED = 20;  // below this, dims  
        int LuxBriLO = 10;  // below this, dims a lot
        int BrightCommand = 250;
        Boolean SetBright = true;
		int LuxMAX 	= 220;
		int Lux1 	= 7;
		int Lux2 	= 9;
		int Lux3 	= 11;
		int Lux4 	= 13;
		int Lux5	= 15;
		int Lux10	= 35;
		int Lux20	= 60;
		int Lux50	=	200;
		int Lux100	=	210;



        // Boolean FR_TempError = false;
        // Boolean RR_TempError = false;

        Boolean FirstPass = true;

        int MainCnt = 0;
        int ModeSleepCnt = 0;

        int FastButtonCnt = 0;
        int BlinkCnt = 0;
        int ErrorWinCnt = 0;

        //  ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ 
        //  These are default if the DEFAULT.XML file is missing
        //  If DEFAULT.XML is present, then it overrides these default values
        //  DEFAULT.XML should be in the /home/pi folder

        Boolean ShowSeconds = false; 
        Boolean DimMore = true;
        Boolean ShowIOStat = true;
        int ModeSleep = 400;  //  4 times a second > i.e. 1 minute = 400  DO NOT set less than 200
        int GenCapacity = 30;
        int GenACCapacity = 20;
        int ShoreCapacity = 30;
        int FRCompAmps = 13;
        int FRBlowerAmps = 3;
        int RRCompAmps = 8;
        int RRBlowerAmps = 3;
        int SHEDDelay = 3;
        int CompBlowerDelay = 3;
        int FRModeRestart = 31;
        int RRModeRestart = 31;
        int FRHardStartDelay = 120;
        int RRHardStartDelay = 120;
        int FR_SurgeDelay = 0;
        int RR_SurgeDelay = 0;
        int SURGEDELAY = 3;
        int HEAT_HYST = 1;
        int AC_HYST = 2;
        Boolean FRUsesGenAC = false;
        Boolean RRUsesGenAC = true;
        int ColdProtect = 0;
        int ColdProtectFR = 0;
        int ColdProtectRR = 0;
        int ColdProtectMin = 0;


        // DEFAULT VALUES
        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


        int FR_FastButtonCnt6 = 0;
        int FR_FastButtonCnt7 = 0;
        int RR_FastButtonCnt6 = 0;
        int RR_FastButtonCnt7 = 0;


        Boolean UsingShorePower;
        Boolean VoltageShorePower;
        Boolean VoltageGennyPower;
        Boolean VoltageGennyACPower;
        Boolean OldVoltageShorePower;
        Boolean OldVoltageGennyPower;
        Boolean OldVoltageGennyACPower;

        int AmpsShorePower;
        int TempAmpsShorePower;
        int AmpsGennyPower;
        int AmpsGennyACPower;
        int MinimumAmps = 1;

        Boolean NoPower = false;

        Boolean FR_Furnace;
        Boolean FR_Compressor;
        Boolean FR_FanHI;
        Boolean FR_FanLO;
        Boolean RR_Furnace;
        Boolean RR_Compressor;
        Boolean RR_FanHI;
        Boolean RR_FanLO;

        Boolean AL_8;
        Boolean AL_7;
        Boolean AL_6;
        Boolean AL_5;
        Boolean AL_4;
        Boolean AL_3;
        Boolean AL_2;
        Boolean AL_1;


        Boolean FR_SHED;
        Boolean FR_CMD;
        int FR_Stat_Indicator_Color;
        int FRMode0_Debounce = 4;
        int FRMode1_Debounce = 4;
        int FRMode3456_Debounce = 4;
        int FR_Blower_Debounce = 4;
        int FR_Comp_Blower_Delay_CNT = 20;
        int FR_HardStart_Counter;
        Boolean FR_HardStartWait;
        Boolean FR_TempError;

        Boolean RR_SHED;
        Boolean RR_CMD;
        int RR_Stat_Indicator_Color;
        int RRMode0_Debounce = 4;
        int RRMode1_Debounce = 4;
        int RRMode3456_Debounce = 4;
        int RR_Blower_Debounce = 4;
        int RR_Comp_Blower_Delay_CNT = 20;
        int RR_HardStart_Counter;
        Boolean RR_HardStartWait;
        Boolean RR_TempError;

        int ModeDebounce = 10;
        int Blower_Delay = 15;

        int StatTextWinReset;
        int TxWatchDog;
        int RxWatchDog;
        int ReturnWatchDog = 0;
        int OldWatchDog;
        int OldReturnWatchDog = 0;

        int WDTest = 0;
        int RWDTest = 0;
        int WDReturn = 0;
        Boolean WDError = true;
        Boolean RWDError = true;
        Boolean CommError;
        Boolean ShowSERStat = false;

        String TempString1;
        String TempString2;

        String LocalIP = "----";
        int IPAddressReconnect = 60;
        int wifireconnect;

        int Alarm_Hour = 6;
        int Alarm_Minute = 30;
        Boolean Alarm_AM = true;
        int Alarm_Duration = 60;
        int Alarm_Mode = 0;  // 0 = OFF,  1 = One Day, 2 = Every Day
        int TZCode = 4;
        String TZName = "Eastern";
        int TempInt;

        Color DisplayColor;
        String DisplayText;
        Boolean BlinkFlag;

        int TimeSecond;
        int OldTimeSecond;

        int i;
         
        public MainScreen()
        {
            InitializeComponent();
        }

       

        private void Form1_Load(object sender, EventArgs e)
        {

         try{
                 myport.NewLine = "\n\r";
                 myport.Open();
            }
            catch (Exception ex)
            {
                LB_TextStatus.Text = ex.Message.ToString();
                StatTextWinReset = 0;
            }

    // FirstPass = false;
                GB_MODE.Visible = false;
                GB_SPLASH.Visible = true;
                GB_CLOCK.Visible = false;
                GB_TimeSet.Visible = false;
                GB_Quit.Visible = false;
                GB_Status.Visible = false;

            MainTimer.Enabled = true;

                UpdateDisplay();
                // Display250();
                SetBright = true;
                SetBrightness();

        }

        void MainTimer_Tick(object sender, EventArgs e)
        {
            MainCnt = MainCnt + 1;
            BlinkCnt = BlinkCnt + 1;
            ModeSleepCnt = ModeSleepCnt + 1;
            FastButtonCnt = FastButtonCnt + 1;
            ErrorWinCnt = ErrorWinCnt + 1;

            if (FirstPass)
            {
                // FR_Temp.ForeColor = Color.PowderBlue;
                // RR_Temp.ForeColor = Color.PowderBlue;
                // OAT_Temp.ForeColor = Color.PowderBlue;

                FR_Temp.Text = "*";
                RR_Temp.Text = "-";
                OAT_Temp.Text = "-";

                StatTextWinReset = 15;
                if (ModeSleepCnt == 3) {

                    LB_TextStatus.Text = "BOOT";

                    ReadXMLFile();
                    ReadDefaultFile();

                LB_Version.Text = "Version > " + VersionNum;
                LB_VersionDate.Text = "Date > " + VersionDate;
                LB_Ras_Version.Text = "RAS > " + VersionNum;

                PB_FRCont.BackColor = Color.Black;
                PB_RRCont.BackColor = Color.Black;

                FR_HardStart_Counter = FRHardStartDelay;
                RR_HardStart_Counter = RRHardStartDelay;

                FRMode0_Debounce = ModeDebounce;
                FRMode1_Debounce = ModeDebounce;
                FRMode3456_Debounce = ModeDebounce;
                FR_Blower_Debounce = ModeDebounce;
                FR_Furnace = false;
                FR_Compressor = false;
                FR_FanHI = false;
                FR_FanLO = false;
                FR_HardStartWait = false;

                RRMode0_Debounce = ModeDebounce;
                RRMode1_Debounce = ModeDebounce;
                RRMode3456_Debounce = ModeDebounce;
                RR_Blower_Debounce = ModeDebounce;
                RR_Furnace = false;
                RR_Compressor = false;
                RR_FanHI = false;
                RR_FanLO = false;
                RR_HardStartWait = false;

                AL_8 = false;
                AL_7 = false;
                AL_6 = false;
                AL_5 = false;
                AL_4 = false;
                AL_3 = false;
                AL_2 = false;
                AL_1 = false;

                WDTest = 10;


                }  // if (ModeSleepCnt == 3) {

                if ((ModeSleepCnt > 30))  // turn off splash screen
                {
                    GB_MODE.Visible = false;
                    GB_SPLASH.Visible = false;
                    GB_CLOCK.Visible = true;
                    GB_TimeSet.Visible = false;
                    GB_Quit.Visible = false;
                    GB_Status.Visible = false;
                    // GB_Setup.Visible = false;
                    // StatTextWinReset = 15;
                    FirstPass = false;
                }
                if ((ModeSleepCnt == 30))  // turn off splash screen
                {

                    TempString1 = GetLocalIP();
                    if (TempString1 == "127.0.0.1") 
                    {
                        LocalIP = "----";
                    } else
                    {
                        LocalIP = TempString1;
                    }
                    LB_IP_ADDRESS.Text = LocalIP;
                }
            }
            else
            {

                // check to see if we need to display buttons
                if (((ModeButtonFront != ModeButtonFrontOLD) || (ModeButtonRear != ModeButtonRearOLD)) & (ShowSetupScreen == false))
                {
                    GB_MODE.Visible = true;
                    GB_SPLASH.Visible = false;
                    GB_CLOCK.Visible = false;
                    GB_TimeSet.Visible = false;
                    GB_Quit.Visible = false;
                    GB_Status.Visible = false;
                    // GB_Setup.Visible = false;
                }

                if (ModeSleepCnt == 60)  // Do an autosave after 15 seconds
                {
                    SaveXML();
                }

                if ((ModeSleepCnt > 60) & (ShowSetupScreen == false)) // switch screen back to clock (15 seconds)
                {
                    GB_MODE.Visible = false;
                    GB_SPLASH.Visible = false;
                    GB_CLOCK.Visible = true;
                    GB_TimeSet.Visible = false;
                    GB_Quit.Visible = false;
                    GB_Status.Visible = false;
                    // GB_Setup.Visible = false;
                    ModeButtonFront = false;
                    ModeButtonRear = false;
                    // ModeSleepCnt == 115;
                }

                //TempInt = ModeSleep - 1;
                //if (ModeSleepCnt == TempInt)  // dim display (if we can) 5 min seconds) 
                //{

                //    SetBright = false;
                //    SetBrightness();

                    /*// Display16 dims to 15%,  Display10 dims to 10%
                    if (DimMore) 
                    {
                         Display10();  
                    }
                    else {
                         Display16();
                    }
                    */
                //}


                if ((ModeSleepCnt > ModeSleep) )  // dim display (if we can) 5 min seconds)
                {
                    if (ShowSetupScreen == false)
                    {
                        GB_MODE.Visible = false;
                        GB_SPLASH.Visible = false;
                        GB_CLOCK.Visible = true;
                        GB_TimeSet.Visible = false;
                        GB_Quit.Visible = false;
                        GB_Status.Visible = false;
                        // GB_Setup.Visible = false;
                        // Display16();

                        // we do this once a second 
                        if (MainCnt == 1)
                        {
                            SetBright = false;
                            SetBrightness();
                        }
                    }
                    ModeSleepCnt = (ModeSleep + 1);
                }

                if (ErrorWinCnt > 20) { 
                    //LB_ER.Text = "";
                    ErrorWinCnt = 100;
                }

                if (ShowSetupScreen)
                {
                    ModeSleepCnt = 0;
                }

                ModeButtonFrontOLD = ModeButtonFront;
                ModeButtonRearOLD = ModeButtonRear;
                
                Random random = new Random();
                TxWatchDog = random.Next(0, 9999);

                BlinkDisplay();
                UpdateTimeDisplay();
                UpdateDisplay();
                SetIO();

                 //SerialTx();

                 SerialRx();

                ////   ********   TEST  TEST

                // VoltageShorePower = true;
                // VoltageGennyPower = false;
                // VoltageGennyACPower = false;

                // AmpsGennyPower = 0;
                // AmpsGennyACPower = 0;
                // AmpsShorePower = 32;

                // TempSourceFront = 1;
                // TempSourceRear = 1;
                // TempSourceOutside = 1;

                ////   ********   TEST  TEST

                // TimeSecond = DateTime.Now.Second();
                // if (TimeSecond != OldTimeSecond) {

                    // one second events   +++++++++++++++++++++++++++++++++++
                if (MainCnt > 3) { 
                    // OldTimeSecond = TimeSecond;
                    MainCnt = 0;
                    FR_SurgeDelay = FR_SurgeDelay + 1;
                    RR_SurgeDelay = RR_SurgeDelay + 1;

                    ColdProtectMin = ColdProtect * 60;

                    ColdProtectFR = ColdProtectFR + 1;
                    if (ColdProtectFR > (ColdProtectMin + 70)) {
                        ColdProtectFR = (ColdProtectMin + 70);
                    }

                    ColdProtectRR = ColdProtectRR + 1;
                    if (ColdProtectRR > (ColdProtectMin + 70))
                    {
                        ColdProtectRR = (ColdProtectMin + 70);
                    }


                    IPAddressReconnect = IPAddressReconnect - 1;
                    FR_HardStart_Counter = FR_HardStart_Counter - 1;
                    RR_HardStart_Counter = RR_HardStart_Counter - 1;

                    if (FR_HardStart_Counter < 0)
                    {
                        FR_HardStart_Counter = -1;
                    }
                    if (RR_HardStart_Counter < 0)
                    {
                        RR_HardStart_Counter = -1;
                    }
                    if (FR_SurgeDelay > 100) { FR_SurgeDelay = 100; }
                    if (RR_SurgeDelay > 100) { RR_SurgeDelay = 100; }

                    StatTextWinReset = StatTextWinReset - 1;
                    if (StatTextWinReset < 1) {
                        if (ShowSERStat) {
                            LB_TextStatus.Text = "ARD WD > "+ RxWatchDog.ToString()  + "   RET WD > " + ReturnWatchDog.ToString(); 
                        }
                        else
                        {
                            LB_TextStatus.Text = "";
                            //LB_TextStatus.Text = Lux.ToString();
                            StatTextWinReset = 0;
                        }
                            }

                    // Watchdog test for recieved communications
                    // Serial.println(WatchDog);

                    // WDError = true;
                    // RWDError = true;

                    if (RxWatchDog != OldWatchDog)
                    {
                        OldWatchDog = RxWatchDog;
                        WDError = false;
                        WDTest = 0;
                        CommError = false;
                        // Serial.println(WatchDog);
                    } else
                    {
                        WDTest = WDTest + 1;
                        if (WDTest > 5)
                        {
                            WDTest = 10;
                            WDError = true;
                            CommError = true;
                            LB_TextStatus.Text = "SERIAL ERROR";
                            StatTextWinReset = 5;
                        }

                    }

                    if (ReturnWatchDog != OldReturnWatchDog)
                    {
                        OldReturnWatchDog = ReturnWatchDog;
                        RWDError = false;
                        RWDTest = 0;
                        // CommError = false;
                        // Serial.println(WatchDog);
                    } else
                    {
                        RWDTest = RWDTest + 1;
                        if (RWDTest > 5)
                        {
                            RWDTest = 10;
                            RWDError = true;
                            CommError = true;
                            LB_TextStatus.Text = "RETURN SERIAL ERROR";
                            StatTextWinReset = 5;
                        }
                    }

                    if ((WDError == false) && (RWDError == false)) {
                        // WDTest = 0;
                        CommError = false;
                    }


             
                    if ((VoltageGennyPower == false) && (VoltageShorePower == true))
                    {
                        UsingShorePower = true;
                    }
                    else
                    {
                        UsingShorePower = false;
                    }


                    if ((VoltageGennyPower == false) && (VoltageGennyACPower == false) && (VoltageShorePower == false))
                    {
                        NoPower = true;
                        UsingShorePower = false;
                    }
                    else
                    {
                        NoPower = false;
                    }

                    if (IPAddressReconnect < 0)
                    {
                        IPAddressReconnect = 60;
                        TempString1 = GetLocalIP();
                        if (TempString1 == "127.0.0.1")
                        {
                            LocalIP = "----";
                        }
                        else
                        {
                            LocalIP = TempString1;
                        }
                        LB_IP_ADDRESS.Text = LocalIP;
                    }

                    SerialTx();
                }  //  if (MainCnt > 5) { 


            } // else first pass


        }  //  MainTimer_Tick(object sender, EventArgs e)

        void SetBrightness()
        {
            if (SetBright)
            {
                BrightCommand = LuxMAX;
            }
            else
            {

                //if (Lux < LuxHI) { BrightCommand = LuxBriHI; }
                //if (Lux < LuxMED) { BrightCommand = LuxBriMED; }
                //if (Lux < LuxLO) { BrightCommand = LuxBriLO; }
				
				
				
				if (Lux < 1) { BrightCommand = Lux1; }
				if (Lux < 2) { BrightCommand = Lux2; }
				if (Lux < 3) { BrightCommand = Lux3; }
				if (Lux < 4) { BrightCommand = Lux4; }
				if (Lux < 5) { BrightCommand = Lux5; }
				if (Lux < 10) { BrightCommand = Lux10; }				
				if (Lux < 20) { BrightCommand = Lux20; }
				if (Lux < 50) { BrightCommand = Lux50; }
				if (Lux < 100) { BrightCommand = Lux100; }
		
				
            }

            try
            {
                StreamWriter sw = new StreamWriter("/sys/class/backlight/rpi_backlight/brightness");

                //Write a line of text
                sw.WriteLine(BrightCommand.ToString());

                //Write a second line of text
                // sw.WriteLine("From the StreamWriter class");

                //Close the file
                sw.Close();
            }
            catch(Exception e)
                {
                // Console.WriteLine("Exception: " + e.Message);
                LB_TextStatus.Text = "Exception: " + e.Message;
                BrightCommand = 9999;
                StatTextWinReset = 15;
                ErrorWinCnt = 0;
            }
            finally 
                {
                // Console.WriteLine("Executing finally block.");
                // LB_TextStatus.Text = "Bright > " + BrightCommand.ToString();
                // StatTextWinReset = 15;
                // ErrorWinCnt = 0;
                LB_BrightCommand.Text = "LUX: " + Lux.ToString() + "  CMD: " + BrightCommand.ToString();
            }

        }

        void UpdateDisplay()
        {
            CheckLimits();
            // LB_TextStatus.Text = ModeSleepCnt.ToString();

            // FR_Temp.Text = HVAC_FR.ToString();
            // RR_Temp.Text = HVAC_RR.ToString();
            // OAT_Temp.Text = HVAC_OAT.ToString();

            if ((HVAC_FR == -60) || (HVAC_FR == 160))
            {
                FR_Temp.Text = "*";
            } else
            {
                FR_Temp.Text = HVAC_FR.ToString();
            }

            if ((HVAC_RR == -60) || (HVAC_RR == 160))
            {
                RR_Temp.Text = "*";
            }
            else
            {
                RR_Temp.Text = HVAC_RR.ToString();
            }

            if ((HVAC_OAT == -60) || (HVAC_OAT == 160))
            {
                OAT_Temp.Text = "-";
            }
            else
            {
                OAT_Temp.Text = HVAC_OAT.ToString();
            }





            LB_Shore_Amps.Text = AmpsShorePower.ToString();
            LB_Gen_Amps.Text = AmpsGennyPower.ToString();
            LB_GenAC_Amps.Text = AmpsGennyACPower.ToString();

            LB_Shore_Volts.ForeColor = Color.DarkGray;
            LB_Genny_Volts.ForeColor = Color.DarkGray;
            LB_GennyAC_Volts.ForeColor = Color.DarkGray;

            if (VoltageShorePower) { LB_Shore_Volts.ForeColor = Color.LimeGreen; }
            if (VoltageGennyPower) { LB_Genny_Volts.ForeColor = Color.LimeGreen; } 
            if (VoltageGennyACPower) { LB_GennyAC_Volts.ForeColor = Color.LimeGreen; }

            LB_TS_Hour.Text = Alarm_Hour.ToString();
            LB_TS_Min.Text = Alarm_Minute.ToString();
            LB_SoundTime.Text = Alarm_Duration.ToString();
            if (Alarm_AM == true) { LB_TS_AMPM.Text = "AM"; }
            if (Alarm_AM == false) { LB_TS_AMPM.Text = "PM"; }
            if (Alarm_Mode == 0) { LB_AlarmMode.Text = "OFF"; }
            if (Alarm_Mode == 1) { LB_AlarmMode.Text = "ONCE"; }
            if (Alarm_Mode == 2) { LB_AlarmMode.Text = "DAILY"; }


            switch (FRMode)
            {
                case 0:
                    // FRStat.Text = "OFF";
                    DisplayText = "OFF";
                    // FRStat.ForeColor = Color.Red;
                    DisplayColor = Color.Red;
                    FRMode0();
                    break;
                case 1:
                    DisplayText = HEATFSP.ToString() + "  HEAT";
                    DisplayColor = Color.OrangeRed;
                    FRMode1();
                    break;
                case 2:
                    DisplayText = "FAN HIGH";
                    DisplayColor = Color.MediumTurquoise;
                    FRMode2345();
                    break;
                case 3:
                    DisplayText = "FAN LOW";
                    DisplayColor = Color.MediumTurquoise;
                    FRMode2345();
                    break;
                case 4:
                    DisplayText = ACFSP.ToString() + "  A/C HIGH";
                    DisplayColor = Color.DeepSkyBlue;
                    FRMode2345();
                    break;
                case 5:
                    DisplayText = ACFSP.ToString() + "  A/C LOW";
                    DisplayColor = Color.DeepSkyBlue;
                    FRMode2345();
                    break;
            }  //  switch (FRMode)

            if (NoPower && BlinkFlag && ((FRMode == 2) || (FRMode == 3) || (FRMode == 4) || (FRMode == 5)))
            // if (BlinkFlag)
            {
                DisplayText = "POWER";
                DisplayColor = Color.Red;
            }

            if (FR_TempError && BlinkFlag && ((FRMode == 1) || (FRMode == 4) || (FRMode == 5)))
            {
                DisplayText = "TEMP ERROR";
                DisplayColor = Color.Red;
            }

            FRStat.Text = DisplayText;
            FRStat.ForeColor = DisplayColor;
            // LB_TextStatus.Text = BlinkFlag.ToString();
            // StatTextWinReset = 15;



            switch (RRMode)
            {
                case 0:
                    DisplayText = "OFF";
                    DisplayColor = Color.Red;
                    RRMode0();
                    break;
                case 1:
                    DisplayText = HEATRSP.ToString() + "  HEAT";
                    DisplayColor = Color.OrangeRed;
                    RRMode1();
                    break;
                case 2:
                    DisplayText = "FAN HIGH";
                    DisplayColor = Color.MediumTurquoise;
                    RRMode2345();
                    break;
                case 3:
                    DisplayText = "FAN LOW";
                    DisplayColor = Color.MediumTurquoise;
                    RRMode2345();
                    break;
                case 4:
                    DisplayText = ACRSP.ToString() + "  A/C HIGH";
                    DisplayColor = Color.DeepSkyBlue;
                    RRMode2345();
                    break;
                case 5:
                    DisplayText = ACRSP.ToString() + "  A/C LOW";
                    DisplayColor = Color.DeepSkyBlue;
                    RRMode2345();
                    break;
            }
            if (NoPower && BlinkFlag && ((RRMode == 2) || (RRMode == 3) || (RRMode == 4) || (RRMode == 5)))
            // if (BlinkFlag)
            {
                DisplayText = "POWER";
                DisplayColor = Color.Red;
            }

            if (RR_TempError && BlinkFlag && ((RRMode == 1) || (RRMode == 4) || (RRMode == 5)))
            {
                DisplayText = "TEMP ERROR";
                DisplayColor = Color.Red;
            }

            RRStat.Text = DisplayText;
            RRStat.ForeColor = DisplayColor;
            // LB_TextStatus.Text = BlinkFlag.ToString();
            // StatTextWinReset = 15;


        }

        void BU_OFF_Click(object sender, EventArgs e)
        {
            if (ModeButtonFront) { FRMode = 0; }
            if (ModeButtonRear) { RRMode = 0; }
            ModeSleepCnt = 0;
            UpdateDisplay();

        }

        void BU_HEAT_Click(object sender, EventArgs e)
        {
            if (ModeButtonFront) { FRMode = 1; }
            if (ModeButtonRear) { RRMode = 1; }
            ModeSleepCnt = 0;
            UpdateDisplay();
        }

        void BU_FANHI_Click(object sender, EventArgs e)
        {
            if (ModeButtonFront) { FRMode = 2; }
            if (ModeButtonRear) { RRMode = 2; }
            ModeSleepCnt = 0;
            UpdateDisplay();
        }

        void BU_FANLO_Click(object sender, EventArgs e)
        {
            if (ModeButtonFront) { FRMode = 3; }
            if (ModeButtonRear) { RRMode = 3; }
            ModeSleepCnt = 0;
            UpdateDisplay();
        }

        void BU_ACHI_Click(object sender, EventArgs e)
        {
            if (ModeButtonFront) { FRMode = 4; }
            if (ModeButtonRear) { RRMode = 4; }
            ModeSleepCnt = 0;
            UpdateDisplay();
        }

        void BU_ACLO_Click(object sender, EventArgs e)
        {
            if (ModeButtonFront) { FRMode = 5; }
            if (ModeButtonRear) { RRMode = 5; }
            ModeSleepCnt = 0;
            UpdateDisplay();
        }

        void BU_UP_Click(object sender, EventArgs e)
        {
            if (ModeButtonFront)
            {
                if (FRMode == 1)
                {
                    HEATFSP = HEATFSP + 1;
                }
                if ((FRMode == 4) || (FRMode == 5))
                {
                    ACFSP = ACFSP + 1;
                }
                ModeSleepCnt = 0;
                UpdateDisplay();
            }

            if (ModeButtonRear) 
            {
                if (RRMode == 1)
                {
                    HEATRSP = HEATRSP + 1;
                }
                if ((RRMode == 4) || (RRMode == 5))
                {
                    ACRSP = ACRSP + 1;
                }
                ModeSleepCnt = 0;
                UpdateDisplay();

            }

        }

        void BU_DOWN_Click(object sender, EventArgs e)
        {
            if (ModeButtonFront)
            {
                if (FRMode == 1)
                {
                    HEATFSP = HEATFSP - 1;
                }
                if ((FRMode == 4) || (FRMode == 5))
                {
                    ACFSP = ACFSP = ACFSP - 1;
                }
                ModeSleepCnt = 0;
                UpdateDisplay();
            }

            if (ModeButtonRear)
            {
                if (RRMode == 1)
                {
                    HEATRSP = HEATRSP - 1;
                }
                if ((RRMode == 4) || (RRMode == 5))
                {
                    ACRSP = ACRSP - 1;
                }
                ModeSleepCnt = 0;
                UpdateDisplay();

            }
        }

        void CheckLimits()
        {
            /* Limit set points 45 - 90 */
            if (HEATFSP < 45)
            {
                HEATFSP = 45;
            }
            if (HEATFSP > 90)
            {
                HEATFSP = 90;
            }
            if (HEATRSP < 45)
            {
                HEATRSP = 45;
            }
            if (HEATRSP > 90)
            {
                HEATRSP = 90;
            }

            if (ACFSP < 45)
            {
                ACFSP = 45;
            }
            if (ACFSP > 90)
            {
                ACFSP = 90;
            }
            if (ACRSP < 45)
            {
                ACRSP = 45;
            }
            if (ACRSP > 90)
            {
                ACRSP = 90;
            }

            // check temperature sensors

            if ((HVAC_FR == -60) || (HVAC_FR == 160) ) { FR_TempError = true; } else { FR_TempError = false; }
            if ((HVAC_RR == -60) || (HVAC_RR == 160) ) { RR_TempError = true; } else { RR_TempError = false; }


        }  //  public void CheckLimits()

        void SaveXML()
        {

          try {
            XmlTextWriter writer = new XmlTextWriter("/home/pi/WaiterECC/CONFIG.XML", null);

            //Write the root element
            writer.WriteStartElement("items");
                
            //Write sub-elements
            writer.WriteElementString("FR_HEAT", HEATFSP.ToString());
            writer.WriteElementString("RR_HEAT", HEATRSP.ToString());
            writer.WriteElementString("FR_AC", ACFSP.ToString());
            writer.WriteElementString("RR_AC", ACRSP.ToString());

            writer.WriteElementString("FRMode", FRMode.ToString());
            writer.WriteElementString("RRMode", RRMode.ToString());

                writer.WriteElementString("ALMHOUR", Alarm_Hour.ToString());
                writer.WriteElementString("ALMMIN", Alarm_Minute.ToString());

                writer.WriteElementString("ALMDURATION", Alarm_Duration.ToString());
                writer.WriteElementString("ALMMODE", Alarm_Mode.ToString());
                writer.WriteElementString("ALMAM", Alarm_AM.ToString());



                // end the root element
                writer.WriteEndElement();

            //Write the XML to file and close the writer
            writer.Close();
          } catch {
                //LB_ER.Text = "ERROR - Write File System";
                LB_TextStatus.Text = "ERROR - CONFIG.XML Write Error";
                StatTextWinReset = 15;
                ErrorWinCnt = 0;
            }
        }

        void BU_SETUP_Click(object sender, EventArgs e)
        {
            SaveXML();
        }

        void ReadXMLFile()
        {
        try {
            XmlTextReader reader = new XmlTextReader("/home/pi/WaiterECC/CONFIG.XML");
            reader.Read();
            while (reader.Read())
            {
                if (reader.Name == "FR_HEAT")
                {
                    HEATFSP = Convert.ToInt16(reader.ReadString());
                }
                else if (reader.Name == "RR_HEAT")
                {
                    HEATRSP = Convert.ToInt16(reader.ReadString());
                }
                else if (reader.Name == "FR_AC")
                {
                    ACFSP = Convert.ToInt16(reader.ReadString());
                }
                else if (reader.Name == "RR_AC")
                {
                    ACRSP = Convert.ToInt16(reader.ReadString());
                }
                else if (reader.Name == "FRMode")
                {
                    FRMode = Convert.ToInt16(reader.ReadString());
                }
                else if (reader.Name == "RRMode")
                {
                    RRMode = Convert.ToInt16(reader.ReadString());
                }
                    else if (reader.Name == "ALMHOUR")
                    {
                        Alarm_Hour = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "ALMMIN")
                    {
                        Alarm_Minute = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "ALMDURATION")
                    {
                        Alarm_Duration = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "ALMMODE")
                    {
                        Alarm_Mode = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "ALMAM")
                    {
                        Alarm_AM = Convert.ToBoolean(reader.ReadString());
                    }

                    // LB_TS_Hour.Text = Alarm_Hour.ToString();
                    // LB_TS_Min.Text = Alarm_Minute.ToString();
                    // LB_SoundTime.Text = Alarm_Duration.ToString();
                    // if (Alarm_AM) { LB_TS_AMPM.Text = "PM"; }
                    // if (Alarm_Mode == 0) { LB_AlarmMode.Text = "OFF"; }
                    // if (Alarm_Mode == 0) { LB_AlarmMode.Text = "OFF"; }
                    // if (Alarm_Mode == 1) { LB_AlarmMode.Text = "ONCE"; }
                    // if (Alarm_Mode == 2) { LB_AlarmMode.Text = "DAILY"; }
                }
                reader.Close();
            } catch (Exception ex)
            {
                //LB_ER.Text = "ERROR - Read File System";
                // LB_TextStatus.Text = "ERROR - CONFIG.XML Read Error";
                LB_TextStatus.Text = "CON - " + ex.Message;
                StatTextWinReset = 15;
                StatTextWinReset = 15;
                ErrorWinCnt = 0;
            }

           
            UpdateDisplay();
        }


        void ReadDefaultFile()
        {
            try
            {
                XmlTextReader reader = new XmlTextReader("/home/pi/WaiterECC/DEFAULT.XML");
                // XmlTextReader reader = new XmlTextReader("DEFAULT.XML");
                
                reader.Read();
                while (reader.Read())
                {
                    if (reader.Name == "MODESLEEP")
                    {
                        ModeSleep = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "SHOWSECONDS")
                    {
                        ShowSeconds = Convert.ToBoolean(reader.ReadString());
                    }
                    else if (reader.Name == "DIMMORE")
                    {
                        DimMore = Convert.ToBoolean(reader.ReadString());
                    }
                    else if (reader.Name == "SHOWIOSTAT")
                    {
                        ShowIOStat = Convert.ToBoolean(reader.ReadString());
                    }
                    else if (reader.Name == "FRUSESGEN")
                    {
                        FRUsesGenAC = Convert.ToBoolean(reader.ReadString());
                    }
                    else if (reader.Name == "FRUSESGEN")
                    {
                        RRUsesGenAC = Convert.ToBoolean(reader.ReadString());
                    }
                    else if (reader.Name == "SHORECAP")
                    {
                        ShoreCapacity = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "GENCAP")
                    {
                        GenCapacity = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "GENACCAP")
                    {
                        GenACCapacity = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "FRCOMPAMPS")
                    {
                        FRCompAmps = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "RRCOMPAMPS")
                    {
                        RRCompAmps = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "FRBLOWERAMPS")
                    {
                        FRBlowerAmps = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "RRBLOWERAMPS")
                    {
                        RRBlowerAmps = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "FRHARDSTARTDELAY")
                    {
                        FRHardStartDelay = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "SHEDDELAY")
                    {
                        SHEDDelay = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "TEMPSOURCEFRONT")
                    {
                        TempSourceFront = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "TEMPSOURCEREAR")
                    {
                        TempSourceRear = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "TEMPSOURCEOUTSIDE")
                    {
                        TempSourceOutside = Convert.ToInt16(reader.ReadString());
                    }

                    else if (reader.Name == "HEAT_HYST")
                    {
                        HEAT_HYST = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "AC_HYST")
                    {
                        AC_HYST = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "SURGEDELAY")
                    {
                        SURGEDELAY = Convert.ToInt16(reader.ReadString());
                    }

                    else if (reader.Name == "SHOWSERSTAT")
                    {
                        ShowSERStat = Convert.ToBoolean(reader.ReadString());
                    }
                    else if (reader.Name == "LUXBRIMAX")
                    {
                        LuxBriMAX = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "LUXBRIHI")
                    {
                        LuxBriHI = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "LUXBRIMED")
                    {
                        LuxBriMED = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "LUXBRILO")
                    {
                        LuxBriLO = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "LUXHI")
                    {
                        LuxHI = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "LUXMED")
                    {
                        LuxMED = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "LUXLO")
                    {
                        LuxLO = Convert.ToInt16(reader.ReadString());
                    }
                    else if (reader.Name == "COLDPROTECT")
                    {
                        ColdProtect = Convert.ToInt16(reader.ReadString());
                    }

                }
                reader.Close();
            }
            catch (Exception ex)
            {
                // LB_Alarm.Text = "ERROR - DEFAULT.XML Read Error";
                ErrorWinCnt = 0;
                // LB_TextStatus.Text = "ERROR - DEFAULT.XML Read Error";
                LB_TextStatus.Text = LB_TextStatus.Text + "DEF - " + ex.Message;
                StatTextWinReset = 15;
            }


            // UpdateDisplay();
        }


        void FR_GB_Clicked()
        {
            if (ShowSetupScreen == false) { 
              ModeButtonFront = true;
              ModeButtonRear = false;
              ModeSleepCnt = 0;
              UpdateDisplay();
                SetBright = true;
                SetBrightness();
                // Display250();
                // Display16();
            }
        }

        void RR_GB_Clicked()
        {
            if (ShowSetupScreen == false)
            {
                ModeButtonFront = false;
                ModeButtonRear = true;
                ModeSleepCnt = 0;
                UpdateDisplay();
                SetBright = true;
                SetBrightness();
                // Display250();
            }
        }

        void SC_GB_Clicked()
        {
            ShowSetupScreen = true;
            //Display250();
            GB_MODE.Visible = false;
            GB_SPLASH.Visible = false;
            GB_CLOCK.Visible = false;
            GB_TimeSet.Visible = true;
            GB_Quit.Visible = false;
            GB_Status.Visible = false;
            // GB_Setup.Visible = false;
            ModeButtonFront = false;
            ModeButtonRear = false;
            // ModeSleepCnt == 115;

        }

        void RR_Temp_Click(object sender, EventArgs e)
        {
            RR_GB_Clicked();
        }

        void RRStat_Click(object sender, EventArgs e)
        {
            RR_GB_Clicked();
        }

        void label2_Click(object sender, EventArgs e)
        {
            RR_GB_Clicked();
        }

        void FR_GBLabel_Click(object sender, EventArgs e)
        {
            FR_GB_Clicked();
        }

        void FR_Temp_Click(object sender, EventArgs e)
        {
            FR_GB_Clicked();
        }

        void FRStat_Click(object sender, EventArgs e)
        {
            FR_GB_Clicked();
        }

      void UpdateTimeDisplay()
        {
            LB_Day.Text = DateTime.Now.ToString("dddd");
            LB_Date.Text = DateTime.Now.ToString("MMMM d" + ", " + "yyyy");
            if (ShowSeconds)
            {
                LB_Time.Text = DateTime.Now.ToString("h:mm:ss");
                // LB_ampm.Left = 380;
                // var point = new Point(320,93);
                // this.LB_ampm.Location = point;
            }
            else
            {
                LB_Time.Text = DateTime.Now.ToString("h:mm");
                // LB_ampm.Left = 380;
                // var point = new Point(270, 93);
                // this.LB_ampm.Location = point;
            }

            LB_ampm.Text = DateTime.Now.ToString("tt");

        }

       void BlinkDisplay()
        {
            if (BlinkCnt > 2)
            {
                if (ModeButtonFront) { FR_GBLabel.Visible = false;  }
                if (ModeButtonRear) { RR_GBLabel.Visible = false; }
                BlinkFlag = true;
            } else
            { // Turn them back on
                FR_GBLabel.Visible = true;
                RR_GBLabel.Visible = true;
                BlinkFlag = false;
            }
            if (BlinkCnt > 5) { BlinkCnt = 0;  }
        }

  
        void FRMode0()
        {
            // Serial.println("Mode 0");
            FRMode0_Debounce = (FRMode0_Debounce - 1);
            if (FRMode0_Debounce < 0)
            {

                FRMode0_Debounce = 0;
                FRMode1_Debounce = ModeDebounce;
                FRMode3456_Debounce = ModeDebounce;
                FR_Blower_Debounce = ModeDebounce;
                FR_Furnace = false;
                FR_Compressor = false;
                FR_FanHI = false;
                FR_FanLO = false;
                FR_CMD = false;
                FR_SHED = false;
                FR_HardStartWait = false;
                FR_Comp_Blower_Delay_CNT = Blower_Delay;

            }
        }

        /* Front Heat   */
        void FRMode1()
        {
            FRMode1_Debounce = (FRMode1_Debounce - 1);
            if (FRMode1_Debounce < 0)
            {

                FRMode0_Debounce = ModeDebounce;
                FRMode1_Debounce = 0;
                FRMode3456_Debounce = ModeDebounce;
                FR_Blower_Debounce = ModeDebounce;
                FR_Comp_Blower_Delay_CNT = Blower_Delay;

                FR_Compressor = false;
                FR_FanHI = false;
                FR_FanLO = false;
                FR_HardStartWait = false;
                FR_CMD = false;
                FR_SHED = false;

                if (HVAC_FR < HEATFSP)
                {
                    FR_Furnace = true;
                    ColdProtectFR = 0;
                    // FR_CMD = true;
                }

                if (HVAC_FR > (HEATFSP + HEAT_HYST))
                {
                    FR_Furnace = false;
                    // FR_CMD = false;
                }

                if ((HEATFSP == 45) & (HVAC_OAT < 32) & (ColdProtect > 0))
                {
                    if (ColdProtectFR > ColdProtectMin)
                    {
                        FR_Furnace = true;
                    } 

                    if (ColdProtectFR > (ColdProtectMin + 60))
                    {
                        FR_Furnace = false;
                        ColdProtectFR = 0;
                    }
                    // ColdProtectFR = 0;
                    // FR_CMD = true;
                } else
                {
                    ColdProtectFR = 0;
                }
            }
            // We're in Mode 1,  but there is a problem with the temperatur sensor, so make sure the furnace is OFF
            if (FR_TempError)
            {
                FRMode0_Debounce = ModeDebounce;
                FRMode1_Debounce = ModeDebounce;
                FRMode3456_Debounce = ModeDebounce;
                FR_Blower_Debounce = ModeDebounce;
                FR_Comp_Blower_Delay_CNT = Blower_Delay;

                FR_Compressor = false;
                FR_FanHI = false;
                FR_FanLO = false;
                FR_HardStartWait = false;
                FR_CMD = false;
                FR_SHED = false;
                FR_Furnace = false;
            }
        }  //  void FRMode1(void)

        /* Front AC Blower   */
        void FRMode2345()
        {
            if (NoPower == false)
            {
                FRMode3456_Debounce = (FRMode3456_Debounce - 1);
                if (FRMode3456_Debounce < 0)
                {

                    FRMode0_Debounce = ModeDebounce;
                    FRMode1_Debounce = ModeDebounce;
                    FRMode3456_Debounce = -1;
                    FR_Furnace = false;
                    // Serial.println("Mode 3456");

                    /* The blower is now ready to start running */
                    if ((FRMode == 2) || (FRMode == 4))
                    {
                        FR_FanLO = false;
                        FR_FanHI = true;
                        // Serial.print("2 4 ");
                    }

                    if ((FRMode == 3) || (FRMode == 5))
                    {
                        FR_FanLO = true;
                        FR_FanHI = false;
                        // Serial.print("3 5 ");
                    }
                    // Serial.print(FRMode);
                    //  Serial.print(" ");
                    // Serial.print(FR_FanLO);
                    // Serial.println(FR_HardStart_Counter);


                    FR_Comp_Blower_Delay_CNT = FR_Comp_Blower_Delay_CNT - 1;
                    if (FR_Comp_Blower_Delay_CNT < 0)
                    {
                        /* Its now OK to run the compressor */
                        FR_Comp_Blower_Delay_CNT = -1;

                        if ((FRMode == 4) || (FRMode == 5))
                        {

                            if (FR_Compressor)
                            {

                                if (HVAC_FR < (ACFSP - 1))
                                {
                                    FR_Compressor = false;
                                    FR_SHED = false;
                                    FR_CMD = false;
                                    FR_HardStartWait = false;
                                    // FR_HardStart_Counter = FRHardStartDelay;
                                }
                                else
                                {
                                    if (FR_SurgeDelay > SURGEDELAY)
                                    {
                                        /* monitor current and temperature */
                                        if ((AmpsShorePower > ShoreCapacity) || (AmpsGennyPower > GenCapacity))
                                        {
                                            FR_SHED = true;
                                            FR_Compressor = false;
                                            // FR_HardStart_Counter = FRHardStartDelay;

                                        }

                                        FR_SurgeDelay = 100;
                                    }
                                }
                            }
                            else
                            {  // if (FR_Compressor)

                                /* Compressor not running, need to start it  */
                                /* Check to see if Hard Start delay is finished */
                                if (HVAC_FR > ACFSP)
                                {
                                    FR_CMD = true;

                                    if (FR_HardStart_Counter < 0)
                                    {
                                        FR_HardStartWait = false;
                                    }
                                    else
                                    { // if (FR_HardStart_Counter < 0)
                                        FR_HardStartWait = true;
                                    }

                                    if (UsingShorePower)
                                    {
                                        if ((AmpsShorePower + FRCompAmps) > ShoreCapacity)
                                        {
                                            FR_SHED = true;
                                        }
                                        else
                                        {
                                            FR_SHED = false;
                                        }
                                    }
                                    else
                                    {
                                        if ((AmpsGennyPower + FRCompAmps) > GenCapacity)
                                        {
                                            FR_SHED = true;
                                        }
                                        else
                                        {
                                            FR_SHED = false;
                                        }
                                    }

                                }
                                else
                                { //  if (HVAC_FR > ACFSP)
                                    /*  If temp drops low before cmpressor kicks on  */
                                    if (HVAC_FR < (ACFSP - 1))
                                    {
                                        FR_Compressor = false;
                                        FR_SHED = false;
                                        FR_CMD = false;
                                        FR_HardStartWait = false;
                                        // FR_HardStart_Counter = RRHardStartDelay;
                                    }
                                }//  if (HVAC_FR > ACFSP)
                            } // else {  // if (FR_Compressor)



                        }
                        else
                        { // if ((FRMode == 4) || (FRMode == 5))
                            FR_Compressor = false;
                            FR_CMD = false;
                            FR_SHED = false;
                        }

                        if ((FR_CMD) && (FR_HardStartWait == false) && (FR_SHED == false))
                        {
                            FR_Compressor = true;
                            FR_HardStart_Counter = FRHardStartDelay;
                            FR_SurgeDelay = 0;
                        }
                        else
                        {
                            FR_Compressor = false;
                        }
                    } // if (FR_Comp_Blower_Delay_CNT < 0)
                } // if  (FRMode3456_Debounce < 0)
            }
            else
            {  // NoPower
                FR_Compressor = false;
                FR_CMD = false;
                FR_SHED = false;
                FR_HardStartWait = false;
                FR_Blower_Debounce = ModeDebounce;
                FR_HardStart_Counter = (FRHardStartDelay / 2);
                FRMode0_Debounce = ModeDebounce;
                FRMode1_Debounce = ModeDebounce;
                FR_Comp_Blower_Delay_CNT = Blower_Delay;
            }  // if nopower

        } // void FRMode3456(void)



        void RRMode0()
        {
            // Serial.println("Mode 0");
            RRMode0_Debounce = (RRMode0_Debounce - 1);
            if (RRMode0_Debounce < 0)
            {

                RRMode0_Debounce = 0;
                RRMode1_Debounce = ModeDebounce;
                RRMode3456_Debounce = ModeDebounce;
                RR_Blower_Debounce = ModeDebounce;
                RR_Furnace = false;
                RR_Compressor = false;
                RR_FanHI = false;
                RR_FanLO = false;
                RR_CMD = false;
                RR_SHED = false;
                RR_HardStartWait = false;
                RR_Comp_Blower_Delay_CNT = Blower_Delay;

            }
        }


        /* Rear Heat   */
        void RRMode1()
        {
            RRMode1_Debounce = (RRMode1_Debounce - 1);
            if (RRMode1_Debounce < 0)
            {

                RRMode0_Debounce = ModeDebounce;
                RRMode1_Debounce = 0;
                RRMode3456_Debounce = ModeDebounce;
                RR_Blower_Debounce = ModeDebounce;
                RR_Comp_Blower_Delay_CNT = Blower_Delay;

                RR_Compressor = false;
                RR_FanHI = false;
                RR_FanLO = false;
                RR_HardStartWait = false;
                RR_CMD = false;
                RR_SHED = false;

                if (HVAC_RR < HEATRSP)
                {
                    RR_Furnace = true;
                    ColdProtectRR = 0;
                    // RR_CMD = true;
                }

                if (HVAC_RR > (HEATRSP + HEAT_HYST))
                {
                    RR_Furnace = false;
                    // RR_CMD = false;
                }

                if ((HEATRSP == 45) & (HVAC_OAT < 32) & (ColdProtect > 0))
                {
                    if (ColdProtectRR > ColdProtectMin)
                    {
                        RR_Furnace = true;
                    }

                    if (ColdProtectRR > (ColdProtectMin + 60))
                    {
                        RR_Furnace = false;
                        ColdProtectRR = 0;
                    }
                    // ColdProtectRR = 0;
                    // FR_CMD = true;
                }
                else
                {
                    ColdProtectRR = 0;
                }
            }
            // We're in Mode 1,  but there is a problem with the temperatur sensor, so make sure the furnace is OFF
            if (RR_TempError)
            {
                RRMode0_Debounce = ModeDebounce;
                RRMode1_Debounce = ModeDebounce;
                RRMode3456_Debounce = ModeDebounce;
                RR_Blower_Debounce = ModeDebounce;
                RR_Comp_Blower_Delay_CNT = Blower_Delay;

                RR_Compressor = false;
                RR_FanHI = false;
                RR_FanLO = false;
                RR_HardStartWait = false;
                RR_CMD = false;
                RR_SHED = false;
                RR_Furnace = false;
            }
        }  //  void RRMode1(void)

        /* Rear AC Blower   */
        void RRMode2345()
        {
            if (NoPower == false)
            {
                RRMode3456_Debounce = (RRMode3456_Debounce - 1);
                if (RRMode3456_Debounce < 0)
                {

                    RRMode0_Debounce = ModeDebounce;
                    RRMode1_Debounce = ModeDebounce;
                    RRMode3456_Debounce = -1;
                    RR_Furnace = false;
                    // Serial.println("Mode 3456");

                    /* The blower is now ready to start running */
                    if ((RRMode == 2) || (RRMode == 4))
                    {
                        RR_FanLO = false;
                        RR_FanHI = true;
                        // Serial.print("2 4 ");
                    }

                    if ((RRMode == 3) || (RRMode == 5))
                    {
                        RR_FanLO = true;
                        RR_FanHI = false;
                        // Serial.print("3 5 ");
                    }
                    // Serial.print(RRMode);
                    //  Serial.print(" ");
                    // Serial.print(RR_FanLO);
                    // Serial.println(RR_HardStart_Counter);


                    RR_Comp_Blower_Delay_CNT = RR_Comp_Blower_Delay_CNT - 1;
                    if (RR_Comp_Blower_Delay_CNT < 0)
                    {
                        /* Its now OK to run the compressor */
                        RR_Comp_Blower_Delay_CNT = -1;

                        if ((RRMode == 4) || (RRMode == 5))
                        {

                            if (RR_Compressor)
                            {

                                if (HVAC_RR < (ACRSP - 1))
                                {
                                    RR_Compressor = false;
                                    RR_SHED = false;
                                    RR_CMD = false;
                                    RR_HardStartWait = false;
                                    // RR_HardStart_Counter = RRHardStartDelay;
                                }
                                else
                                {

                                    if (RR_SurgeDelay > SURGEDELAY)
                                    {
                                        /* monitor current and temperature */
                                        if ((AmpsShorePower > ShoreCapacity) || (AmpsGennyPower > GenCapacity))
                                        {
                                            RR_SHED = true;
                                            RR_Compressor = false;
                                            // RR_HardStart_Counter = RRHardStartDelay;

                                        }
                                        RR_SurgeDelay = 100;
                                    }
                                }
                            }
                            else
                            {  // if (RR_Compressor)

                                /* Compressor not running, need to start it  */
                                /* Check to see if Hard Start delay is finished */
                                if (HVAC_RR > ACRSP)
                                {
                                    RR_CMD = true;

                                    if (RR_HardStart_Counter < 0)
                                    {
                                        RR_HardStartWait = false;
                                    }
                                    else
                                    { // if (RR_HardStart_Counter < 0)
                                        RR_HardStartWait = true;
                                    }

                                    if (UsingShorePower)
                                    {
                                        if ((AmpsShorePower + RRCompAmps) > ShoreCapacity)
                                        {
                                            RR_SHED = true;
                                        }
                                        else
                                        {
                                            RR_SHED = false;
                                        }
                                    }
                                    else
                                    {
                                        if ((AmpsGennyPower + RRCompAmps) > GenCapacity)
                                        {
                                            RR_SHED = true;
                                        }
                                        else
                                        {
                                            RR_SHED = false;
                                        }
                                    }

                                }
                                else
                                { //  if (HVAC_RR > ACFSP)
                                    /*  If temp drops low before cmpressor kicks on  */
                                    if (HVAC_RR < (ACRSP - 1))
                                    {
                                        RR_Compressor = false;
                                        RR_SHED = false;
                                        RR_CMD = false;
                                        RR_HardStartWait = false;
                                        // RR_HardStart_Counter = RRHardStartDelay;
                                    }
                                }


                            } // else {  // if (RR_Compressor)



                        }
                        else
                        { // if ((RRMode == 4) || (RRMode == 5))
                            RR_Compressor = false;
                            RR_CMD = false;
                            RR_SHED = false;
                        }
                        if ((RR_CMD) && (RR_HardStartWait == false) && (RR_SHED == false))
                        {
                            RR_Compressor = true;
                            RR_HardStart_Counter = RRHardStartDelay;
                            RR_SurgeDelay = 0;
                        }
                        else
                        {
                            RR_Compressor = false;
                        }
                    } // if (RR_Comp_Blower_Delay_CNT < 0)
                } // if  (RRMode3456_Debounce < 0)

            }
            else
            {  // NoPower
                RR_Compressor = false;
                RR_CMD = false;
                RR_SHED = false;
                RR_HardStartWait = false;
                RR_Blower_Debounce = ModeDebounce;
                RR_HardStart_Counter = (RRHardStartDelay / 2);
                RRMode0_Debounce = ModeDebounce;
                RRMode1_Debounce = ModeDebounce;
                RR_Comp_Blower_Delay_CNT = Blower_Delay;
            }  // if nopower
        }

        void SetIO()
        {
            PB_FRCont.BackColor = Color.Black;

            if ((FRMode == 4) || (FRMode == 5))
            {
                if (FR_CMD)
                {
                    PB_FRCont.BackColor = Color.Lime;
                }
                if (FR_SHED)
                {
                    PB_FRCont.BackColor = Color.Red;
                }
                if (FR_HardStartWait && FR_CMD && (FR_SHED == false))
                {
                    PB_FRCont.BackColor = Color.Blue ;
                }
            }
            if (FR_Furnace)
            {
                PB_FRCont.BackColor = Color.Lime;
            }

            PB_RRCont.BackColor = Color.Black;

            if ((RRMode == 4) || (RRMode == 5))
            {
                if (RR_CMD)
                {
                    PB_RRCont.BackColor = Color.Lime;
                }
                if (RR_SHED)
                {
                    PB_RRCont.BackColor = Color.Red;
                }
                if (RR_HardStartWait && RR_CMD && (RR_SHED == false))
                {
                    PB_RRCont.BackColor = Color.Blue ;
                }
            }
            if (RR_Furnace)
            {
                PB_RRCont.BackColor = Color.Lime;
            }

            if (ShowIOStat)
            {
                // PB_FRCont.Visible = false;
                // PB_RRCont.Visible = false;

                if (FR_Furnace) { FR_1.BackColor = Color.Lime; } else { FR_1.BackColor = Color.Silver; }
                if (FR_Compressor) { FR_2.BackColor = Color.Lime; } else { FR_2.BackColor = Color.Silver; }
                if (FR_FanHI) { FR_3.BackColor = Color.Lime; } else { FR_3.BackColor = Color.Silver; }
                if (FR_FanLO) { FR_4.BackColor = Color.Lime; } else { FR_4.BackColor = Color.Silver; }

                if (RR_Furnace) { RR_1.BackColor = Color.Lime; } else { RR_1.BackColor = Color.Silver; }
                if (RR_Compressor) { RR_2.BackColor = Color.Lime; } else { RR_2.BackColor = Color.Silver; }
                if (RR_FanHI) { RR_3.BackColor = Color.Lime; } else { RR_3.BackColor = Color.Silver; }
                if (RR_FanLO) { RR_4.BackColor = Color.Lime; } else { RR_4.BackColor = Color.Silver; }

            }
            else
            {
                // PB_FRCont.Visible = true;
                // PB_RRCont.Visible = true;

                FR_1.BackColor = Color.Black;
                FR_2.BackColor = Color.Black;
                FR_3.BackColor = Color.Black;
                FR_4.BackColor = Color.Black;
                RR_1.BackColor = Color.Black;
                RR_2.BackColor = Color.Black;
                RR_3.BackColor = Color.Black;
                RR_4.BackColor = Color.Black;

                //FR_1.BackColor = Color.Black;
                //FR_2.Visible = false;
                //FR_3.Visible = false;
                //FR_4.Visible = false;
                //RR_1.Visible = false;
                //RR_2.Visible = false;
                //RR_3.Visible = false;
                //RR_4.Visible = false;
            }


            // tft.fillRect(165, 170, 10, 10, RR_Stat_Indicator_Color);

        }

        private void button3_Click(object sender, EventArgs e)
        {  //  SETUP EXIT
            ShowSetupScreen = false; 
            GB_MODE.Visible = false;
            GB_SPLASH.Visible = false;
            GB_CLOCK.Visible = true;
            GB_TimeSet.Visible = false;
            GB_Quit.Visible = false;
            GB_Status.Visible = false;
            // GB_Setup.Visible = false;
            // SaveXML();
            ModeButtonFront = false;
            ModeButtonRear = false;
            ModeSleepCnt = 0;
        }

        /*
        void Display250()
        {
            var info = new ProcessStartInfo();
            info.FileName = "screen250.sh";
            // info.FileName = "sudo";
            // info.Arguments = "rpi-backlight max";

            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            try
            {
                var p = Process.Start(info);
    
            p.WaitForExit(50);
        }
            catch { }
            // Console.ReadLine();
            if (ModeSleepCnt > 150) { ModeSleepCnt = 150; }
        }

        void Display16()
        {

            var info = new ProcessStartInfo();
            info.FileName = "screen16.sh";
            // info.FileName = "sudo";
            // info.Arguments = "rpi-backlight min";

            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            try
            {
                var p = Process.Start(info);
        
            p.WaitForExit(50);
            }
            catch { }
            // Console.ReadLine();
        }

        void Display10()
        {

            var info = new ProcessStartInfo();
            info.FileName = "screen10.sh";
            // info.Arguments = "-E sh -c  'echo 15 > /sys/class/backlight/rpi_backlight/brightness'";

            // info.FileName = "/sys/class/backlight/rpi_backlight/brightness";
            //  info.Arguments = "15";

            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            try
            {
                var p = Process.Start(info);
         
            p.WaitForExit(50);
            }
            catch { }
            // Console.ReadLine();
        }

    */
        
        private void label5_Click(object sender, EventArgs e)
        {
             // Display250();
            SetBright = true;
            SetBrightness();
        }

        void SerialRx()
        {
            try
            {
                TempString2 = "";
                int nByte = myport.BytesToRead;
                 for (i = 0; i < nByte; i++)
                {
                     int byteTemp = myport.ReadByte();
                     TempString2 += (char)(byteTemp);
                }
            }
            catch (Exception ex)
            {
                LB_TextStatus.Text = ex.Message.ToString();
                StatTextWinReset = 0;
            }

            // Find WD[
            if (TempString2.Length > 0) 
            {
                
                TempInt = getBetween(TempString2, "Version[", "]");

                TempInt = getBetween(TempString2, "WD[", "]");
                if (TempInt > -1000) { RxWatchDog = TempInt; }

                TempInt = getBetween(TempString2, "WR[", "]");
                if (TempInt > -1000) { ReturnWatchDog = TempInt; }

                // NOTE - Temp sensor set at -60 if its reporting incorectly
                TempInt = getBetween(TempString2, "T1[", "]");
                if ((TempInt > -50) && (TempInt < 150)) 
                {
                    if (TempSourceFront == 1) { HVAC_FR = TempInt; }
                    if (TempSourceRear == 1) { HVAC_RR = TempInt; }
                    if (TempSourceOutside == 1) { HVAC_OAT = TempInt; }
                    Temp1Cnt = 0;
                } 
                else
                {
                    Temp1Cnt = Temp1Cnt + 1;
                    if (Temp1Cnt > 30)
                    {
                        Temp1Cnt = 60;
                        if (TempSourceFront == 1) { HVAC_FR = TempInt; }
                        if (TempSourceRear == 1) { HVAC_RR = TempInt; }
                        if (TempSourceOutside == 1) { HVAC_OAT = TempInt; }
                    }
                }

                TempInt = getBetween(TempString2, "T2[", "]");
                if ((TempInt > -50) && (TempInt < 150))
                {
                    if (TempSourceFront == 2) { HVAC_FR = TempInt; }
                    if (TempSourceRear == 2) { HVAC_RR = TempInt; }
                    if (TempSourceOutside == 2) { HVAC_OAT = TempInt; }
                    Temp2Cnt = 0;
                }
                else
                {
                    Temp2Cnt = Temp2Cnt + 1;
                    if (Temp2Cnt > 30)
                    {
                        Temp2Cnt = 60;
                        if (TempSourceFront == 2) { HVAC_FR = TempInt; }
                        if (TempSourceRear == 2) { HVAC_RR = TempInt; }
                        if (TempSourceOutside == 2) { HVAC_OAT = TempInt; }
                    }
                }


                TempInt = getBetween(TempString2, "T3[", "]");
                if ((TempInt > -50) && (TempInt < 150))
                {
                    if (TempSourceFront == 3) { HVAC_FR = TempInt; }
                    if (TempSourceRear == 3) { HVAC_RR = TempInt; }
                    if (TempSourceOutside == 3) { HVAC_OAT = TempInt; }
                    Temp3Cnt = 0;
                }
                else
                {
                    Temp3Cnt = Temp3Cnt + 1;
                    if (Temp3Cnt > 30)
                    {
                        Temp3Cnt = 60;
                        if (TempSourceFront == 3) { HVAC_FR = TempInt; }
                        if (TempSourceRear == 3) { HVAC_RR = TempInt; }
                        if (TempSourceOutside == 3) { HVAC_OAT = TempInt; }
                    }
                }


                TempInt = getBetween(TempString2, "SV[", "]");
                if (TempInt > -1000)
                {
                    if (TempInt == 0) { VoltageShorePower = false; }
                    if (TempInt == 1) { VoltageShorePower = true; }
                }

                TempInt = getBetween(TempString2, "GV[", "]");
                if (TempInt > -1000)
                {
                    if (TempInt == 0) { VoltageGennyPower = false; }
                    if (TempInt == 1) { VoltageGennyPower = true; }
                }

                TempInt = getBetween(TempString2, "RV[", "]");
                if (TempInt > -1000)
                {
                    if (TempInt == 0) { VoltageGennyACPower = false; }
                    if (TempInt == 1) { VoltageGennyACPower = true; }
                }

                TempInt = getBetween(TempString2, "SA[", "]");
                if (TempInt > -1000) { AmpsShorePower = TempInt; }

                TempInt = getBetween(TempString2, "GA[", "]");
                if (TempInt > -1000) { AmpsGennyPower = TempInt; }

                TempInt = getBetween(TempString2, "RA[", "]");
                if (TempInt > -1000) { AmpsGennyACPower = TempInt; }

                TempInt = getBetween(TempString2, "LX[", "]");
                if ((TempInt > 0) && (TempInt < 5000)) { Lux = TempInt; }
                LB_LuxRpt.Text = Lux.ToString();
                //Lux = 15;

            }
            if (WDError)
            {
                 
                 VoltageShorePower = false;
                 VoltageGennyPower = false;
                 VoltageGennyACPower = false;

                 AmpsGennyPower = 0;
                 AmpsGennyACPower = 0;
                 AmpsShorePower = 0;

                 HVAC_FR = -60;
                 HVAC_RR = -60;
                 HVAC_OAT = -60;
                 
            }

        }



        void SerialTx()
        {
            
            try
            {
                TempString1 = "Version[" + VersionNum + "]";
                 myport.WriteLine(TempString1);
                TempString1 = "Date[" + VersionDate + "]";
                 myport.WriteLine(TempString1);
                TempString1 = "WD[" + TxWatchDog.ToString() + "]";
                 myport.WriteLine(TempString1);


                TempInt = 0;
                if (FR_Furnace) { TempInt = TempInt + 0b10000000; }
                if (FR_Compressor) { TempInt = TempInt + 0b01000000; }
                if (FR_FanHI) { TempInt = TempInt + 0b00100000; }
                if (FR_FanLO) { TempInt = TempInt + 0b00010000; }
                if (RR_Furnace) { TempInt = TempInt + 0b00001000; }
                if (RR_Compressor) { TempInt = TempInt + 0b00000100; }
                if (RR_FanHI) { TempInt = TempInt + 0b00000010; }
                if (RR_FanLO) { TempInt = TempInt + 0b00000001; }

                TempString1 = "RL[" + TempInt.ToString() + "]";
                 myport.WriteLine(TempString1);

                TempInt = 0;
                if (AL_8) { TempInt = TempInt + 0b10000000; }
                if (AL_7) { TempInt = TempInt + 0b01000000; }
                if (AL_6) { TempInt = TempInt + 0b00100000; }
                if (AL_5) { TempInt = TempInt + 0b00010000; }
                if (AL_4) { TempInt = TempInt + 0b00001000; }
                if (AL_3) { TempInt = TempInt + 0b00000100; }
                if (AL_2) { TempInt = TempInt + 0b00000010; }
                if (AL_1) { TempInt = TempInt + 0b00000001; }

                TempString1 = "AL[" + TempInt.ToString() + "]";
                 myport.WriteLine(TempString1);

            }
            catch (Exception ex)
            {
                LB_TextStatus.Text = ex.Message.ToString();
                StatTextWinReset = 0;
            }

        }  //  void SerialTx()

    

        private void LB_Day_Click(object sender, EventArgs e)
        {
            // if ((ModeSleepCnt > 1200)) { ModeSleepCnt = 150; }
             //Display250();
            SetBright = true;
            SetBrightness();
        }

        private void LB_Date_Click(object sender, EventArgs e)
        {
            // if ((ModeSleepCnt > 1200)) { ModeSleepCnt = 150; }
            // Display250(); 
            SetBright = true;
            SetBrightness();
        }

        private void LB_Time_Click(object sender, EventArgs e)
        {
            // if ((ModeSleepCnt > 1200)) { ModeSleepCnt = 150; }
            //Display250(); 
            SetBright = true;
            SetBrightness();
        }


        private void GB_CLOCK_Enter(object sender, EventArgs e)
        {
            // if ((ModeSleepCnt > 1200)) { ModeSleepCnt = 150; }
            //Display250();
            SetBright = true;
            SetBrightness();
        }

        private void BU_SetupCancel_Click(object sender, EventArgs e)
        {  // setup cancel
            ShowSetupScreen = false;
            GB_MODE.Visible = false;
            GB_SPLASH.Visible = false;
            GB_CLOCK.Visible = true;
            GB_TimeSet.Visible = false;
            GB_Quit.Visible = false;
            // GB_Setup.Visible = false;
            ModeButtonFront = false;
            ModeButtonRear = false;
            ModeSleepCnt = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            ShowSetupScreen = true;
            GB_MODE.Visible = false;
            GB_SPLASH.Visible = false;
            GB_CLOCK.Visible = false;
            GB_TimeSet.Visible = false;
            GB_Quit.Visible = false;
            GB_Status.Visible = false;
            // GB_Setup.Visible = true;
            ModeButtonFront = false;
            ModeButtonRear = false;
            // Display250();
            SetBright = true;
            SetBrightness();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {

        }

        private void BU_TimeSetScreen_Click(object sender, EventArgs e)
        {
             // Display250();
            ShowSetupScreen = true;
            GB_MODE.Visible = false;
            GB_SPLASH.Visible = false;
            GB_CLOCK.Visible = false;
            GB_TimeSet.Visible = true;
            GB_Quit.Visible = false;
            GB_Status.Visible = false;
            // GB_Setup.Visible = false;
            ModeButtonFront = false;
            ModeButtonRear = false;
            //Display250();
            // ModeSleepCnt = 125;
            SetBright = true;
            SetBrightness();
        }

        private void BU_Alarm_Time_set_Click(object sender, EventArgs e)
        {
            // Display250();
            ShowSetupScreen = true;
            GB_MODE.Visible = false;
            GB_SPLASH.Visible = false;
            GB_CLOCK.Visible = false;
            GB_TimeSet.Visible = true;
            GB_Quit.Visible = false;
            GB_Status.Visible = false;
            // GB_Setup.Visible = false;
            ModeButtonFront = false;
            ModeButtonRear = false;
            // Display250();
            // ModeSleepCnt = 125;
            SetBright = true;
            SetBrightness();
        }

        public static int getBetween(string strSource, string strStart, string strEnd)
        // string text = "This is an example string and my data is here";
        // string data = getBetween(text, "my", "is");
        //  It returns the string or nothing
        {
            int Start, End, x;
            String TS = "-1000";
            String TS1 = "";

            if (strSource.Contains(strStart))
            {
                Start = strSource.IndexOf(strStart, 0);
                // TS = Start.ToString();
                TS1 = strSource.Substring(Start);


                if (TS1.Contains(strEnd))
                {
                    End = TS1.IndexOf(strEnd, 0);
                    TS = TS1.Substring(3,(End-3));
                    // TS = TS1.Substring(3);
                    // if (strStart == "Version[") { LB_Ard_Version.Text = TS; }

                }
                else
                {
                    TS = "-1000";
                }

                
            }
            try
            {
                x = Convert.ToInt16(TS);
            }
            catch
            {
                x = -1000;
            }
            return x;
        
        }
        public static string GetLocalIP()
        {
            string ipv4Address = String.Empty;

            foreach (IPAddress currentIPAddress in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (currentIPAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipv4Address = currentIPAddress.ToString();
                    break;
                }
            }

            return ipv4Address;
        }

        public void GetTimeZone()
        {
    
                if (TZCode == 0) {TZName = "Alaska"; }
                if (TZCode == 1) {TZName = "Arizona"; }
                if (TZCode == 4) {TZName = "Eastern"; }
                if (TZCode == 6) {TZName = "Michigan"; }
                if (TZCode == 8) {TZName = "Pacific"; }
                if (TZCode == 2) {TZName = "Central"; }
                if (TZCode == 3) {TZName = "East-Indiana"; }
                if (TZCode == 5) {TZName = "Indiana - Starke"; }
                if (TZCode == 7) {TZName = "Mountain"; }
                if (TZCode == 9) {TZName = "Pacific-New"; }

            LB_TimeZone.Text = TZName;

        }

        private void BU_QUIT_Click(object sender, EventArgs e)
        {
            // Display250();
            ShowSetupScreen = true;
            GB_MODE.Visible = false;
            GB_SPLASH.Visible = false;
            GB_CLOCK.Visible = false;
            GB_TimeSet.Visible = false;
            GB_Quit.Visible = true;
            GB_Status.Visible = false;
            // GB_Setup.Visible = false;
            ModeButtonFront = false;
            ModeButtonRear = false;
            // System.Environment.Exit(1);
        }

        private void BU_SetTime_Click(object sender, EventArgs e)
        {
            if (LB_IP_ADDRESS.Text != "----") {
                // set the RTC to match the system clock.
                // The system clock updates when it gets a NTS server.
                var info = new ProcessStartInfo();
                info.FileName = "TIME_SYS2RTC.sh";
                // info.Arguments = "-E sh -c  'echo 15 > /sys/class/backlight/rpi_backlight/brightness'";

                // info.FileName = "/sys/class/backlight/rpi_backlight/brightness";
                //  info.Arguments = "15";

                info.UseShellExecute = false;
                info.CreateNoWindow = true;

                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;

                var p = Process.Start(info);
                p.WaitForExit(500);
            }
            // Console.ReadLine();
        }

        private void BU_QuitYes_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(1);
        }

        private void BU_QuitNo_Click(object sender, EventArgs e)
        {
            ShowSetupScreen = false;
            GB_MODE.Visible = false;
            GB_SPLASH.Visible = false;
            GB_CLOCK.Visible = true;
            GB_TimeSet.Visible = false;
            GB_Quit.Visible = false;
            GB_Status.Visible = false;
            // GB_Setup.Visible = false;
            ModeButtonFront = false;
            ModeButtonRear = false;
            ModeSleepCnt = 0;
        }

        private void LB_TS_Hour_Click(object sender, EventArgs e)
        {
            Alarm_Hour = Alarm_Hour + 1;
            if (Alarm_Hour > 12) { Alarm_Hour = 0;  }
            LB_TS_Hour.Text = Alarm_Hour.ToString();
            // LB_TS_Min.Text = Alarm_Minute.ToString();
            // LB_SoundTime.Text = Alarm_Duration.ToString();
            // if (Alarm_AM) { LB_TS_AMPM.Text = "PM"; }
            // if (Alarm_Mode == 0) { LB_AlarmMode.Text = "OFF"; }
        }

        private void LB_TS_Min_Click(object sender, EventArgs e)
        {
            Alarm_Minute = Alarm_Minute + 5;
            if (Alarm_Minute > 59) { Alarm_Minute = 0; }
            LB_TS_Min.Text = Alarm_Minute.ToString();
        }

        private void LB_TS_AMPM_Click(object sender, EventArgs e)
        {
            if (Alarm_AM)
            {
                Alarm_AM = false;
                LB_TS_AMPM.Text = "PM";
            } else
            {
                Alarm_AM = true;
                LB_TS_AMPM.Text = "AM";
            }
        }

        private void LB_SoundTime_Click(object sender, EventArgs e)
        {
            Alarm_Duration = Alarm_Duration + 30;
            if (Alarm_Duration > 150) { Alarm_Duration = 30;  }
            LB_SoundTime.Text = Alarm_Duration.ToString();
        }

        private void LB_AlarmMode_Click(object sender, EventArgs e)
        {
            // Alarm_Mode = 0;  // 0 = OFF,  1 = One Day, 2 = Every Day
            Alarm_Mode = Alarm_Mode + 1;
            if (Alarm_Mode > 2) { Alarm_Mode = 0;  }
            if (Alarm_Mode == 0) { LB_AlarmMode.Text = "OFF"; }
            if (Alarm_Mode == 1) { LB_AlarmMode.Text = "ONCE"; }
            if (Alarm_Mode == 2) { LB_AlarmMode.Text = "DAILY"; }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            ShowSetupScreen = false;
            GB_MODE.Visible = false;
            GB_SPLASH.Visible = false;
            GB_CLOCK.Visible = true;
            GB_TimeSet.Visible = false;
            GB_Quit.Visible = false;
            GB_Status.Visible = false;
            // GB_Setup.Visible = false;
            // SaveXML();
            ModeButtonFront = false;
            ModeButtonRear = false;
            ModeSleepCnt = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Display250();
            ShowSetupScreen = true;
            GB_MODE.Visible = false;
            GB_SPLASH.Visible = false;
            GB_CLOCK.Visible = false;
            GB_TimeSet.Visible = false;
            GB_Quit.Visible = false;
            GB_Status.Visible = true;
            // GB_Setup.Visible = false;
            ModeButtonFront = false;
            ModeButtonRear = false;
            // Display250();
            // ModeSleepCnt = 125;
            SetBright = true;
            SetBrightness();
        }

        private void LB_TimeZone_Click(object sender, EventArgs e)
        {
            TZCode = TZCode + 1;
            if (TZCode > 9) {TZCode = 0;  }
            GetTimeZone();
        }
    }  // public partial class MainScreen : Form

}  //  namespace WindowsFormsApplication1




