using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using PhasmoGadget.HotKeyAPI.Contracts;
using PhasmoGadget.HotKeyAPI.Services;
using PhasmoGadget.HotKeyAPI.Structs;
using PhasmoGadget.PhasmoGadget.Properties;

namespace PhasmoGadget.PhasmoGadget {
    partial class PhasmoGadget {
        private void Main_Load(object sender,EventArgs e) {
            PhasmoGadget.SetWindowPos(this.Handle,PhasmoGadget.HWND_TOPMIST,0,0,0,0,3U);
            this.set_size_and_scale();
            this.RoundedCorners();
            this.Visible=false;
            this.Visible=true;
            this.lblFocus.Focus();
        }
        private void panel_settings_load() {
            this.set_font_size_options_panel();
            this.SetBackgroundSettings();
            this.UserIconImages();
            this.Visible=true;

            this.tbKeyID.Text=Settings.Default["KeyID"].ToString();
            this.tbTimerID.Text=Settings.Default["TimerID"].ToString();
            this.cbLanguage.SelectedIndex=(int)Settings.Default["Lang"];
            int int32=Convert.ToInt32((double)Settings.Default["Opacity"]/5.0);
            this.trbOpacity.Value=int32;
            this.lblOpacity_Value.Text=(int32*5).ToString()+"%";
            this.trbSize.Value=(int)Settings.Default["Size"];
            this.lblSize_Value.Text=this.trbSize.Value.ToString();

            this.tbTextColorR.Text=Settings.Default["TextColorR"].ToString();
            this.tbTextColorG.Text=Settings.Default["TextColorG"].ToString();
            this.tbTextColorB.Text=Settings.Default["TextColorB"].ToString();
            this.tbBoxForeColorR.Text=Settings.Default["BoxForeColorR"].ToString();
            this.tbBoxForeColorG.Text=Settings.Default["BoxForeColorG"].ToString();
            this.tbBoxForeColorB.Text=Settings.Default["BoxForeColorB"].ToString();
            this.tbBoxBackColorR.Text=Settings.Default["BoxBackColorR"].ToString();
            this.tbBoxBackColorG.Text=Settings.Default["BoxBackColorG"].ToString();
            this.tbBoxBackColorB.Text=Settings.Default["BoxBackColorB"].ToString();
            this.tbHintTextColorR.Text=Settings.Default["HintTextColorR"].ToString();
            this.tbHintTextColorG.Text=Settings.Default["HintTextColorG"].ToString();
            this.tbHintTextColorB.Text=Settings.Default["HintTextColorB"].ToString();
            this.tbBackgroundColorR.Text=Settings.Default["BackgroundColorR"].ToString();
            this.tbBackgroundColorG.Text=Settings.Default["BackgroundColorG"].ToString();
            this.tbBackgroundColorB.Text=Settings.Default["BackgroundColorB"].ToString();

            this.set_rgb_boxes();
        }

        private void RoundedCorners() {
            this.FormBorderStyle=FormBorderStyle.None;
            this.Region=Region.FromHrgn(PhasmoGadget.CreateRoundRectRgn(0,0,this.Width+1,this.Height+1,20,20));
        }

        private void activate_HotKey() {
            this.hkRecorder=(IHotKeyRecorder)new HotKeyRecorder();
            this.hkFactory=(IHotKeyFactory)new HotKeyFactory();
            this.hkListener=(IHotKeyListener)new HotKeyListener();
            this.hkListener.OnHotKeyPressed+=new EventHandler<HotKeyPressedEventArgs>(this.OnHotKeyPressed);
            this.hkRecorder.CleanHotKeys();

            TypeConverter converter=TypeDescriptor.GetConverter(typeof(Keys));
            HotKey hotKey1=this.hkFactory.CreateHotKey((int)converter.ConvertFromString(Settings.Default["KeyID"].ToString())).GetHotKey();
            HotKey hotKey2=this.hkFactory.CreateHotKey((int)converter.ConvertFromString(Settings.Default["TimerID"].ToString())).GetHotKey();
            this.hkRecorder.RegisterHotKey(hotKey1);
            this.hkRecorder.RegisterHotKey(hotKey2);
            if(!this.hkListener.IsListening)
                this.hkListener.StartListening();
        }
        private void OnHotKeyPressed(object sender,HotKeyPressedEventArgs e) {
            TypeConverter converter=TypeDescriptor.GetConverter(typeof(Keys));
            if((Keys)converter.ConvertFromString(e.HotKey.KeyCode.ToString())==(Keys)converter.ConvertFromString(Settings.Default["KeyID"].ToString())) {
                ++this.var_HotKeyPressed;
                if(this.WindowState==FormWindowState.Normal && this.var_HotKeyPressed>=2) {
                    this.WindowState=FormWindowState.Minimized;
                    this.var_HotKeyPressed=0;
                } else {
                    if(this.WindowState==FormWindowState.Minimized && this.var_HotKeyPressed>=2){
                        this.WindowState=FormWindowState.Normal;
                        Cursor.Position=new Point(this.Location.X+this.Size.Width/2+10,this.Location.Y+this.Size.Height/2);

                        if((int)Settings.Default["CompactMode"]==1) {
                            Point location=this.Location;
                            int x1=location.X;
                            location=this.btnMin.Location;
                            int x2=location.X;
                            int x3=x1+x2+30;

                            location=this.Location;
                            int y1=location.Y;
                            location=this.btnMin.Location;
                            int y2=location.Y;
                            int y3=y1+y2+60;

                            Cursor.Position=new Point(x3,y3);
                        }
                        this.var_HotKeyPressed=0;
                        this.lblFocus.Focus();
                    }
                }
            } else {
                if((Keys)converter.ConvertFromString(e.HotKey.KeyCode.ToString())!=(Keys)converter.ConvertFromString(Settings.Default["TimerID"].ToString()))
                    return;
                ++this.var_HotKeyPressed;
                if(this.cbTimerType.SelectedIndex!=1) {
                    if(!this.timer.Enabled && this.var_HotKeyPressed>=2) {
                        if(this.var_seconds>0) {
                            this.tbCounter.BackColor=ColorTranslator.FromHtml("#60d68a");
                            this.timer.Enabled=true;
                            this.timer.Start();
                            if((int)Settings.Default["SoundOnOff"]==1)
                                this.beep_1.Play();
                            this.lblFocus.Focus();
                        }
                        this.var_HotKeyPressed=0;
                    } else {
                        if(!this.timer.Enabled || this.var_HotKeyPressed<2)
                            return;
                        this.tbCounter.BackColor=Color.WhiteSmoke;
                        this.timer.Stop();
                        this.cbTimerType_SelectedIndexGetIndex(this.cbTimerType);
                        if(this.custom_timer!=0 &&this.cbTimerType.SelectedIndex==0) {
                            this.tbCounter.Text=this.custom_timer.ToString();
                            this.var_seconds=int.Parse(this.tbCounter.Text)-1;
                        }
                        this.lblFocus.Focus();
                        this.var_HotKeyPressed=0;
                    }
                } else {
                    if(this.cbTimerType.SelectedIndex!=1)
                        return;
                    if(!this.timer.Enabled &&this.var_HotKeyPressed>=2) {
                        this.tbCounter.Text="00:00";
                        this.var_seconds=0;
                        this.tbCounter.BackColor=ColorTranslator.FromHtml("#60d68a");
                        this.timer.Enabled=true;
                        this.timer.Start();
                        if((int)Settings.Default["SoundOnOff"]==1)
                            this.beep_1.Play();
                        this.lblFocus.Focus();
                        this.var_HotKeyPressed=0;
                    } else {
                        if(!this.timer.Enabled || this.var_HotKeyPressed<2)
                            return;
                        this.tbCounter.BackColor=Color.WhiteSmoke;
                        this.timer.Stop();
                        this.cbTimerType_SelectedIndexGetIndex(this.cbTimerType);
                        this.lblFocus.Focus();
                        this.var_HotKeyPressed=0;
                    }
                }
            }
        }

        private void FormPosition() {
            if(Settings.Default["PositionX"]==null && Settings.Default["PositionX"]==null) {
                this.Location=new Point(Screen.PrimaryScreen.Bounds.Width-this.Size.Width);
                Settings.Default["PositionX"]=(object)this.Location.X;
                Settings.Default["PositionY"]=(object)this.Location.Y;
                Settings.Default.Save();
            } else 
                this.Location=new Point((int)Settings.Default["PositionX"],(int)Settings.Default["PositionY"]);
        }
        private void UserColors() {
            int red1=(int)Settings.Default["TextColorR"];
            int green1=(int)Settings.Default["TextColorG"];
            int blue1=(int)Settings.Default["TextColorB"];

            int red2=(int)Settings.Default["BoxForeColorR"];
            int green2=(int)Settings.Default["BoxForeColorG"];
            int blue2=(int)Settings.Default["BoxForeColorB"];

            int red3=(int)Settings.Default["BoxBackColorR"];
            int green3=(int)Settings.Default["BoxBackColorG"];
            int blue3=(int)Settings.Default["BoxBackColorB"];

            int red4=(int)Settings.Default["HintTextColorR"];
            int green4=(int)Settings.Default["HintTextColorG"];
            int blue4=(int)Settings.Default["HintTextColorB"];

            int red5=(int)Settings.Default["BackgroundColorR"];
            int green5=(int)Settings.Default["BackgroundColorG"];
            int blue5=(int)Settings.Default["BackgroundColorB"];

            //Background
            this.BackColor=Color.FromArgb(red5,green5,blue5);
            
            //Check Boxes
            this.chbSoundOnOff.ForeColor=Color.FromArgb(red1,green1,blue1);
            this.chbCompact.ForeColor=Color.FromArgb(red1,green1,blue1);
            
            //Combo Boxes
            this.cbFirstName.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.cbFirstName.BackColor=Color.FromArgb(red3,green3,blue3);
            this.cbLastName.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.cbLastName.BackColor=Color.FromArgb(red3,green3,blue3);
            this.cbAnswerType.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.cbAnswerType.BackColor=Color.FromArgb(red3,green3,blue3);
            this.cbTask1.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.cbTask1.BackColor=Color.FromArgb(red3,green3,blue3);
            this.cbTask2.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.cbTask2.BackColor=Color.FromArgb(red3,green3,blue3);
            this.cbTask3.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.cbTask3.BackColor=Color.FromArgb(red3,green3,blue3);
            this.cbGhosts.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.cbGhosts.BackColor=Color.FromArgb(red3,green3,blue3);
            this.cbInfo.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.cbInfo.BackColor=Color.FromArgb(red3,green3,blue3);
            this.cbTimerType.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.cbTimerType.BackColor=Color.FromArgb(red3,green3,blue3);
            this.cbMaps.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.cbMaps.BackColor=Color.FromArgb(red3,green3,blue3);
            this.cbLanguage.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.cbLanguage.BackColor=Color.FromArgb(red3,green3,blue3);

            //Labels
            this.lblHint.ForeColor=Color.FromArgb(red4,green4,blue4);
            this.lblHotKey.ForeColor=Color.FromArgb(red1,green1,blue1);
            this.lblTimerKey.ForeColor=Color.FromArgb(red1,green1,blue1);
            this.lblLanguage.ForeColor=Color.FromArgb(red1,green1,blue1);
            this.lblOpacity.ForeColor=Color.FromArgb(red1,green1,blue1);
            this.lblOpacity_Value.ForeColor=Color.FromArgb(red1,green1,blue1);
            this.lblCopyright.ForeColor=Color.FromArgb(red1,green1,blue1);
            this.lblVersion.ForeColor=Color.FromArgb(red1,green1,blue1);
            this.lblTextColor.ForeColor=Color.FromArgb(red1,green1,blue1);
            this.lblBoxForeColor.ForeColor=Color.FromArgb(red1,green1,blue1);
            this.lblBoxBackColor.ForeColor=Color.FromArgb(red1,green1,blue1);
            this.lblHintTextColor.ForeColor=Color.FromArgb(red1,green1,blue1);
            this.lblBackgroundColor.ForeColor=Color.FromArgb(red1,green1,blue1);
            this.lblGhInfoType.ForeColor=Color.FromArgb(red1,green1,blue1);
            this.lblGhInfoTypeData.ForeColor=Color.FromArgb(red1,green1,blue1);
            this.lblGhInfoHunt.ForeColor=Color.FromArgb(red1,green1,blue1);
            this.lblGhInfoHuntData.ForeColor=Color.FromArgb(red1,green1,blue1);
            this.lblGhInfoHuntAbilities.ForeColor=Color.FromArgb(red1,green1,blue1);
            this.lblGhInfoHuntAbilitiesData.ForeColor=Color.FromArgb(red1,green1,blue1);
            this.lblGhInfoCooldown.ForeColor=Color.FromArgb(red1,green1,blue1);
            this.lblGhInfoCooldownData.ForeColor=Color.FromArgb(red1,green1,blue1);
            
            //Rich Text Boxes
            this.rtbGhostTyp.ForeColor=Color.FromArgb(red1,green1,blue1);
            this.rtbGhostTyp.BackColor=Color.FromArgb(red5,green5,blue5);
            this.rtbGhInfoHintsData.ForeColor=Color.FromArgb(red1,green1,blue1);
            this.rtbGhInfoHintsData.BackColor=Color.FromArgb(red5,green5,blue5);
            this.rtbInfo.ForeColor=Color.FromArgb(red1,green1,blue1);
            this.rtbInfo.BackColor=Color.FromArgb(red5,green5,blue5);

            //Text Boxes
            this.tbKeyID.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.tbKeyID.BackColor=Color.FromArgb(red3,green3,blue3);
            this.tbTimerID.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.tbTimerID.BackColor=Color.FromArgb(red3,green3,blue3);
            this.tbTextColorR.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.tbTextColorR.BackColor=Color.FromArgb(red3,green3,blue3);
            this.tbTextColorG.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.tbTextColorG.BackColor=Color.FromArgb(red3,green3,blue3);
            this.tbTextColorB.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.tbTextColorB.BackColor=Color.FromArgb(red3,green3,blue3);
            this.tbBoxForeColorR.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.tbBoxForeColorR.BackColor=Color.FromArgb(red3,green3,blue3);
            this.tbBoxForeColorG.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.tbBoxForeColorG.BackColor=Color.FromArgb(red3,green3,blue3);
            this.tbBoxForeColorB.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.tbBoxForeColorB.BackColor=Color.FromArgb(red3,green3,blue3);
            this.tbBoxBackColorR.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.tbBoxBackColorR.BackColor=Color.FromArgb(red3,green3,blue3);
            this.tbBoxBackColorG.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.tbBoxBackColorG.BackColor=Color.FromArgb(red3,green3,blue3);
            this.tbBoxBackColorB.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.tbBoxBackColorB.BackColor=Color.FromArgb(red3,green3,blue3);
            this.tbHintTextColorR.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.tbHintTextColorR.BackColor=Color.FromArgb(red3,green3,blue3);
            this.tbHintTextColorG.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.tbHintTextColorG.BackColor=Color.FromArgb(red3,green3,blue3);
            this.tbHintTextColorB.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.tbHintTextColorB.BackColor=Color.FromArgb(red3,green3,blue3);
            this.tbBackgroundColorR.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.tbBackgroundColorR.BackColor=Color.FromArgb(red3,green3,blue3);
            this.tbBackgroundColorG.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.tbBackgroundColorG.BackColor=Color.FromArgb(red3,green3,blue3);
            this.tbBackgroundColorB.ForeColor=Color.FromArgb(red2,green2,blue2);
            this.tbBackgroundColorB.BackColor=Color.FromArgb(red3,green3,blue3);

            //Track Bars
            this.trbOpacity.BackColor=Color.FromArgb(red5,green5,blue5);
            this.trbSize.BackColor=Color.FromArgb(red5,green5,blue5);
        }
        private void set_rgb_boxes() {
            this.tbTextColor_rgb.BackColor=Color.FromArgb(int.Parse(this.tbTextColorR.Text),int.Parse(this.tbTextColorG.Text),int.Parse(this.tbTextColorB.Text));
            this.tbBoxForeColor_rgb.BackColor=Color.FromArgb(int.Parse(this.tbBoxForeColorR.Text),int.Parse(this.tbBoxForeColorG.Text),int.Parse(this.tbBoxForeColorB.Text));
            this.tbBoxBackColor_rgb.BackColor=Color.FromArgb(int.Parse(this.tbBoxBackColorR.Text),int.Parse(this.tbBoxBackColorG.Text),int.Parse(this.tbBoxBackColorB.Text));
            this.tbHintTextColor_rgb.BackColor=Color.FromArgb(int.Parse(this.tbHintTextColorR.Text),int.Parse(this.tbHintTextColorG.Text),int.Parse(this.tbHintTextColorB.Text));
            this.tbBackgroundColor_rgb.BackColor=Color.FromArgb(int.Parse(this.tbBackgroundColorR.Text),int.Parse(this.tbBackgroundColorG.Text),int.Parse(this.tbBackgroundColorB.Text));
        }
        private void set_focus(object sender,EventArgs e) => this.lblFocus.Focus();
        private void set_size_and_scale() {
            this.set_font_size();
            if((int)Settings.Default["Size"]!=this.var_last_size) {
                if(this.var_last_size==2)
                    this.Scale(new SizeF(0.87f,0.87f));
                else if(this.var_last_size==3)
                    this.Scale(new SizeF(0.8f,0.8f));
                else if(this.var_last_size==4)
                    this.Scale(new SizeF(0.75f,0.75f));

                if((int)Settings.Default["Size"]==1) {
                    if(this.var_toggleInfo==0)
                        this.Size=new Size(380,290);
                    if(this.var_toggleInfo==1)
                        this.Size=(new Size(380,692));
                        this.rtbGhInfoHintsData.Size=new Size(352,224);
                        this.rtbInfo.Size=new Size(355,630);
                }
                if((int)Settings.Default["Size"]==2) {
                    if(this.var_toggleInfo==0){
                        this.Scale(new SizeF(1.15f,1.15f));
                        this.Size=new Size(437,334);
                    }
                    if(this.var_toggleInfo==1){
                        this.Scale(new SizeF(1.15f,1.15f));
                        this.Size=new Size(437,780);
                        this.rtbGhInfoHintsData.Size=new Size(405,244);
                        this.rtbInfo.Size=new Size(405,710);
                    }
                }
                if((int)Settings.Default["Size"]==3) {
                    if(this.var_toggleInfo==0){
                        this.Scale(new SizeF(1.25f,1.25f));
                        this.Size=new Size(475,363);
                    }
                    if(this.var_toggleInfo==1){
                        this.Scale(new SizeF(1.25f,1.25f));
                        this.Size=new Size(475,838);
                        this.rtbGhInfoHintsData.Size=new Size(438,250);
                        this.rtbInfo.Size=new Size(445,760);
                    }
                }
                if((int)Settings.Default["Size"]==4) {
                    if(this.var_toggleInfo==0){
                        this.Scale(new SizeF(1.33421052f,1.33421052f));
                        this.Size=new Size(507,387);
                    }
                    if(this.var_toggleInfo==1){
                        this.Scale(new SizeF(1.33421052f,1.33421052f));
                        this.Size=new Size(507,838);
                        this.rtbGhInfoHintsData.Size=new Size(467,220);
                        this.rtbInfo.Size=new Size(475,760);
                    }
                }
            }

            if((int)Settings.Default["CompactMode"]==1) {
                if((int)Settings.Default["Size"]==1) {
                    if(this.btnBox.Visible) {
                        this.var_toggleInfo=0;
                        this.Size=new Size(230,250);
                        this.RoundedCorners();
                    } else {
                        this.var_toggleInfo=0;
                        this.Size=new Size(380,290);
                        this.RoundedCorners();
                    }
                }else if((int)Settings.Default["Size"]==2) {
                    if(this.btnBox.Visible) {
                        this.var_toggleInfo=0;
                        this.Size=new Size(265,287);
                        this.RoundedCorners();
                    } else {
                        this.var_toggleInfo=0;
                        this.Size=new Size(437,334);
                        this.RoundedCorners();
                    }
                }else if((int)Settings.Default["Size"]==3) {
                    if(this.btnBox.Visible) {
                        this.var_toggleInfo=0;
                        this.Size=new Size(287,313);
                        this.RoundedCorners();
                    } else {
                        this.var_toggleInfo=0;
                        this.Size=new Size(475,363);
                        this.RoundedCorners();
                    }
                }else if((int)Settings.Default["Size"]==4) {
                    if(this.btnBox.Visible) {
                        this.var_toggleInfo=0;
                        this.Size=new Size(305,332);
                        this.RoundedCorners();
                    } else {
                        this.var_toggleInfo=0;
                        this.Size=new Size(507,387);
                        this.RoundedCorners();
                    }
                }
            }
            this.set_components();
            this.set_lblCounter_lblDif_size();
        }
        private void show_info() {
            if((int)Settings.Default["Size"]==1) {
                if(this.var_toggleInfo==0)
                    this.Size=new Size(380,290);
                if(this.var_toggleInfo==1)
                    this.Size=new Size(380,692);
                    this.rtbGhInfoHintsData.Size=new Size(352,224);
                    this.rtbInfo.Size=new Size(355,630);
            }
            if((int)Settings.Default["Size"]==2) {
                if(this.var_toggleInfo==0)
                    this.Size=new Size(437,334);
                if(this.var_toggleInfo==1)
                    this.Size=new Size(437,780);
                    this.rtbGhInfoHintsData.Size=new Size(405,244);
                    this.rtbInfo.Size=new Size(405,710);
            }
            if((int)Settings.Default["Size"]==3) {
                if(this.var_toggleInfo==0)
                    this.Size=new Size(475,363);
                if(this.var_toggleInfo==1)
                    this.Size=new Size(475,838);
                    this.rtbGhInfoHintsData.Size=new Size(438,250);
                    this.rtbInfo.Size=new Size(445,760);
            }
            if((int)Settings.Default["Size"]==4) {
                if(this.var_toggleInfo==0)
                    this.Size=new Size(507,387);
                if(this.var_toggleInfo==1)
                    this.Size=new Size(507,838);
                    this.rtbGhInfoHintsData.Size=new Size(467,220);
                    this.rtbInfo.Size=new Size(475,760);
            }
        }
        private void set_components() {
            if((int)Settings.Default["CompactMode"]==0) {
                this.btnGhostInfo.Show();

                this.cbFirstName.Show();
                this.cbLastName.Show();
                this.cbAnswerType.Show();
                this.cbTask1.Show();
                this.cbTask2.Show();
                this.cbTask3.Show();

                this.lblHint.Show();

                if((int)Settings.Default["Size"]==1) {
                    this.btnEMF.Location=new Point(25,69);
                    this.btnFinger.Location=new Point(145,69);
                    this.btnBook.Location=new Point(85,96);
                    this.btnTemp.Location=new Point(145,123);
                    this.btnOrb.Location=new Point(25,123);
                    this.btnBox.Location=new Point(85,42);
                    this.btnDots.Location=new Point(85,150);
                    this.btnMin.Location=new Point(316,5);
                    this.btnExit.Location=new Point(346,5);
                    this.btnHelp.Location=new Point(256,8);
                    this.btnReset.Location=new Point(348,239);
                    this.btnSettings.Location=new Point(286,8);
                    this.btnGhostInfo.Location=new Point(348,265);

                    this.cbFirstName.Location=new Point(14,7);
                    this.cbLastName.Location=new Point(135,7);
                    this.cbAnswerType.Location=new Point(288,40);
                    this.cbTask1.Location=new Point(226,121);
                    this.cbTask2.Location=new Point(226,148);
                    this.cbTask3.Location=new Point(226,175);
                    this.cbGhosts.Location=new Point(14,300);
                    this.cbTimerType.Location=new Point(226,92);
                    this.cbMaps.Location=new Point(226,66);
                    
                    this.lblDif.Location=new Point(352,66);
                    this.lblHint.Location=new Point(11, 241);
                    this.lblGhInfoType.Location=new Point(14,330);
                    this.lblGhInfoTypeData.Location=new Point(145,330);
                    this.lblGhInfoHunt.Location=new Point(14,364);
                    this.lblGhInfoHuntData.Location=new Point(145,364);
                    this.lblGhInfoHuntAbilities.Location=new Point(14,398);
                    this.lblGhInfoHuntAbilitiesData.Location=new Point(145,398);
                    this.lblGhInfoCooldown.Location=new Point(14,434);
                    this.lblGhInfoCooldownData.Location=new Point(145,434);

                    this.picLine.Location=new Point(15,290);
                    this.picGhostEvi1.Location=new Point(181,295);
                    this.picGhostEvi2.Location=new Point(225,295);
                    this.picGhostEvi3.Location=new Point(269,295);
                    this.picGhostEvi4.Location=new Point(313,295);

                    this.rtbGhostTyp.Location=new Point(14,210);
                    this.rtbGhostTyp.Width=315;
                    this.rtbGhostTyp.Height=70;
                    this.rtbGhInfoHintsData.Location=new Point(14,458);

                    this.tbCounter.Location=new Point(334,92);
                }
                if((int)Settings.Default["Size"]==2) {
                    this.btnEMF.Location=new Point(29,80);
                    this.btnFinger.Location=new Point(167,80);
                    this.btnBook.Location=new Point(98,111);
                    this.btnTemp.Location=new Point(167,142);
                    this.btnOrb.Location=new Point(29,142);
                    this.btnBox.Location=new Point(98,49);
                    this.btnDots.Location=new Point(98,173);
                    this.btnMin.Location=new Point(364,7);
                    this.btnExit.Location=new Point(399,7);
                    this.btnHelp.Location=new Point(295,10);
                    this.btnReset.Location=new Point(401,276);
                    this.btnSettings.Location=new Point(330,10);
                    this.btnGhostInfo.Location=new Point(401,306);

                    this.cbFirstName.Location=new Point(16,10);
                    this.cbLastName.Location=new Point(155,10);
                    this.cbAnswerType.Location=new Point(332,47);
                    this.cbTask1.Location=new Point(261,141);
                    this.cbTask2.Location=new Point(261,172);
                    this.cbTask3.Location=new Point(261,203);
                    this.cbGhosts.Location=new Point(16,345);
                    this.cbTimerType.Location=new Point(261,107);
                    this.cbMaps.Location=new Point(261,77);
                    
                    this.lblDif.Location=new Point(406,77);
                    this.lblHint.Location=new Point(13, 278);
                    this.lblGhInfoType.Location=new Point(16,380);
                    this.lblGhInfoTypeData.Location=new Point(167,380);
                    this.lblGhInfoHunt.Location=new Point(16,419);
                    this.lblGhInfoHuntData.Location=new Point(167,419);
                    this.lblGhInfoHuntAbilities.Location=new Point(16,458);
                    this.lblGhInfoHuntAbilitiesData.Location=new Point(167,458);
                    this.lblGhInfoCooldown.Location=new Point(16,499);
                    this.lblGhInfoCooldownData.Location=new Point(167,499);
                    
                    this.picLine.Location=new Point(17,334);
                    this.picGhostEvi1.Location=new Point(208,339);
                    this.picGhostEvi2.Location=new Point(259,339);
                    this.picGhostEvi3.Location=new Point(309,339);
                    this.picGhostEvi4.Location=new Point(360,339);

                    this.rtbGhostTyp.Location=new Point(16,242);
                    this.rtbGhostTyp.Width=380;
                    this.rtbGhostTyp.Height=80;
                    this.rtbGhInfoHintsData.Location=new Point(16,527);

                    this.tbCounter.Location=new Point(385,107);
                }
                if((int)Settings.Default["Size"]==3) {
                    this.btnEMF.Location=new Point(31,88);
                    this.btnFinger.Location=new Point(181,88);
                    this.btnBook.Location=new Point(106,121);
                    this.btnTemp.Location=new Point(181,155);
                    this.btnOrb.Location=new Point(31,155);
                    this.btnBox.Location=new Point(106,54);
                    this.btnDots.Location=new Point(106,189);
                    this.btnMin.Location=new Point(394,7);
                    this.btnExit.Location=new Point(432,7);
                    this.btnHelp.Location=new Point(320,11);
                    this.btnReset.Location=new Point(435,300);
                    this.btnSettings.Location=new Point(357,11);
                    this.btnGhostInfo.Location=new Point(435,333);

                    this.cbFirstName.Location=new Point(18,10);
                    this.cbLastName.Location=new Point(169,10);
                    this.cbAnswerType.Location=new Point(361,52);
                    this.cbTask1.Location=new Point(283,153);
                    this.cbTask2.Location=new Point(283,187);
                    this.cbTask3.Location=new Point(283,220);
                    this.cbGhosts.Location=new Point(18,375);
                    this.cbTimerType.Location=new Point(283,117);
                    this.cbMaps.Location=new Point(283,85);
                    
                    this.lblDif.Location=new Point(440,85);
                    this.lblHint.Location=new Point(14, 301);
                    this.lblGhInfoType.Location=new Point(18,413);
                    this.lblGhInfoTypeData.Location=new Point(181,413);
                    this.lblGhInfoHunt.Location=new Point(18,455);
                    this.lblGhInfoHuntData.Location=new Point(181,455);
                    this.lblGhInfoHuntAbilities.Location=new Point(18,498);
                    this.lblGhInfoHuntAbilitiesData.Location=new Point(181,498);
                    this.lblGhInfoCooldown.Location=new Point(18,543);
                    this.lblGhInfoCooldownData.Location=new Point(181,543);
                    
                    this.picLine.Location=new Point(19,363);
                    this.picGhostEvi1.Location=new Point(226,369);
                    this.picGhostEvi2.Location=new Point(281,369);
                    this.picGhostEvi3.Location=new Point(336,369);
                    this.picGhostEvi4.Location=new Point(391,369);

                    this.rtbGhostTyp.Location=new Point(17,264);
                    this.rtbGhostTyp.Width=400;
                    this.rtbGhostTyp.Height=80;
                    this.rtbGhInfoHintsData.Location=new Point(18,573);

                    this.tbCounter.Location=new Point(419,117);
                }
                if((int)Settings.Default["Size"]==4) {
                    this.btnEMF.Location=new Point(34,92);
                    this.btnFinger.Location=new Point(194,92);
                    this.btnBook.Location=new Point(114,128);
                    this.btnTemp.Location=new Point(194,164);
                    this.btnOrb.Location=new Point(34,164);
                    this.btnBox.Location=new Point(114,56);
                    this.btnDots.Location=new Point(114,200);
                    this.btnMin.Location=new Point(422,7);
                    this.btnExit.Location=new Point(462,7);
                    this.btnHelp.Location=new Point(342,11);
                    this.btnReset.Location=new Point(465,319);
                    this.btnSettings.Location=new Point(382,11);
                    this.btnGhostInfo.Location=new Point(465,354);

                    this.cbFirstName.Location=new Point(19,9);
                    this.cbLastName.Location=new Point(181,9);
                    this.cbAnswerType.Location=new Point(385,53);
                    this.cbTask1.Location=new Point(302,161);
                    this.cbTask2.Location=new Point(302,197);
                    this.cbTask3.Location=new Point(302,233);
                    this.cbGhosts.Location=new Point(19,400);
                    this.cbTimerType.Location=new Point(302,122);
                    this.cbMaps.Location=new Point(302,87);
                    
                    this.lblDif.Location=new Point(470,87);
                    this.lblHint.Location=new Point(15, 322);
                    this.lblGhInfoType.Location=new Point(19,440);
                    this.lblGhInfoTypeData.Location=new Point(193,440);
                    this.lblGhInfoHunt.Location=new Point(19,486);
                    this.lblGhInfoHuntData.Location=new Point(193,486);
                    this.lblGhInfoHuntAbilities.Location=new Point(19,531);
                    this.lblGhInfoHuntAbilitiesData.Location=new Point(193,531);
                    this.lblGhInfoCooldown.Location=new Point(19,579);
                    this.lblGhInfoCooldownData.Location=new Point(193,579);
                    
                    this.picLine.Location=new Point(20,387);
                    this.picGhostEvi1.Location=new Point(241,394);
                    this.picGhostEvi2.Location=new Point(300,394);
                    this.picGhostEvi3.Location=new Point(359,394);
                    this.picGhostEvi4.Location=new Point(418,394);

                    this.rtbGhostTyp.Location=new Point(19,282);
                    this.rtbGhostTyp.Width=440;
                    this.rtbGhostTyp.Height=80;
                    this.rtbGhInfoHintsData.Location=new Point(19,611);

                    this.tbCounter.Location=new Point(446,122);
                }
                    
            } else if((int)Settings.Default["CompactMode"]==1) {
                this.btnGhostInfo.Hide();

                this.cbFirstName.Hide();
                this.cbLastName.Hide();
                this.cbAnswerType.Hide();
                this.cbTask1.Hide();
                this.cbTask2.Hide();
                this.cbTask3.Hide();

                this.lblHint.Hide();

                if((int)Settings.Default["Size"]==1) {
                    this.btnEMF.Location=new Point(14,91);
                    this.btnFinger.Location=new Point(134,91);
                    this.btnBook.Location=new Point(74,118);
                    this.btnTemp.Location=new Point(134,145);
                    this.btnOrb.Location=new Point(14,145);
                    this.btnBox.Location=new Point(74,64);
                    this.btnDots.Location=new Point(74,172);
                    this.btnMin.Location=new Point(168,5);
                    this.btnExit.Location=new Point(198,5);
                    this.btnHelp.Location=new Point(203,176);
                    this.btnReset.Location=new Point(203,199);
                    this.btnSettings.Location=new Point(203,222);

                    this.cbTimerType.Location=new Point(12,36);
                    this.cbMaps.Location=new Point(12,13);
                    
                    this.lblDif.Location=new Point(138,13);

                    this.rtbGhostTyp.Location=new Point(11,225);
                    this.rtbGhostTyp.Width=180;
                    this.rtbGhostTyp.Height=16;

                    this.tbCounter.Location=new Point(120,36);
                }
                if((int)Settings.Default["Size"]==2) {
                    this.btnEMF.Location=new Point(17,105);
                    this.btnFinger.Location=new Point(155,105);
                    this.btnBook.Location=new Point(86,136);
                    this.btnTemp.Location=new Point(155,167);
                    this.btnOrb.Location=new Point(17,167);
                    this.btnBox.Location=new Point(86,74);
                    this.btnDots.Location=new Point(86,198);
                    this.btnMin.Location=new Point(194,6);
                    this.btnExit.Location=new Point(229,6);
                    this.btnHelp.Location=new Point(234,203);
                    this.btnReset.Location=new Point(234,229);
                    this.btnSettings.Location=new Point(234,255);

                    this.cbTimerType.Location=new Point(14,42);
                    this.cbMaps.Location=new Point(14,16);
                    
                    this.lblDif.Location=new Point(161,16);

                    this.rtbGhostTyp.Location=new Point(13,258);
                    this.rtbGhostTyp.Width=207;
                    this.rtbGhostTyp.Height=20;

                    this.tbCounter.Location=new Point(140,42);
                }
                if((int)Settings.Default["Size"]==3) {
                    this.btnEMF.Location=new Point(17,114);
                    this.btnFinger.Location=new Point(167,114);
                    this.btnBook.Location=new Point(92,147);
                    this.btnTemp.Location=new Point(167,182);
                    this.btnOrb.Location=new Point(17,182);
                    this.btnBox.Location=new Point(92,79);
                    this.btnDots.Location=new Point(92,215);
                    this.btnMin.Location=new Point(209,6);
                    this.btnExit.Location=new Point(247,6);
                    this.btnHelp.Location=new Point(254,220);
                    this.btnReset.Location=new Point(254,249);
                    this.btnSettings.Location=new Point(254,278);

                    this.cbTimerType.Location=new Point(14,44);
                    this.cbMaps.Location=new Point(14,16);
                    
                    this.lblDif.Location=new Point(172,16);

                    this.rtbGhostTyp.Location=new Point(13,282);
                    this.rtbGhostTyp.Width=225;
                    this.rtbGhostTyp.Height=20;

                    this.tbCounter.Location=new Point(150,44);
                }
                if((int)Settings.Default["Size"]==4) {
                    this.btnEMF.Location=new Point(18,120);
                    this.btnFinger.Location=new Point(178,120);
                    this.btnBook.Location=new Point(98,156);
                    this.btnTemp.Location=new Point(178,192);
                    this.btnOrb.Location=new Point(18,192);
                    this.btnBox.Location=new Point(98,84);
                    this.btnDots.Location=new Point(98,228);
                    this.btnMin.Location=new Point(223,6);
                    this.btnExit.Location=new Point(263,6);
                    this.btnHelp.Location=new Point(270,233);
                    this.btnReset.Location=new Point(270,264);
                    this.btnSettings.Location=new Point(270,294);

                    this.cbTimerType.Location=new Point(14,46);
                    this.cbMaps.Location=new Point(14,16);
                    
                    this.lblDif.Location=new Point(182,16);

                    this.rtbGhostTyp.Location=new Point(13,299);
                    this.rtbGhostTyp.Width=240;
                    this.rtbGhostTyp.Height=20;

                    this.tbCounter.Location=new Point(158,46);
                }
            }

            if((int)Settings.Default["Size"]==1) {
                this.btnHintClose.Location=new Point(346,5);
                this.btnSetClose.Location=new Point(346,5);
                this.btnResetColor.Location=new Point(270,192);
                this.btnOK.Location=new Point(318,5);

                this.chbCompact.Location=new Point(107,247);
                this.chbSoundOnOff.Location=new Point(13,247);

                this.cbLanguage.Location=new Point(14,35);
                this.cbInfo.Location=new Point(14,14);

                this.lblHotKey.Location=new Point(10,70);
                this.lblTimerKey.Location=new Point(83,70);
                this.lblLanguage.Location=new Point(10,15);
                this.lblOpacity.Location=new Point(10,187);
                this.lblOpacity_Value.Location=new Point(64,187);
                this.lblSize.Location=new Point(10,126);
                this.lblSize_Value.Location=new Point(44,126);
                this.lblCopyright.Location=new Point(276,263);
                this.lblVersion.Location=new Point(285,235);
                this.lblTextColor.Location=new Point(194,68);
                this.lblBoxForeColor.Location=new Point(165,92);
                this.lblBoxBackColor.Location=new Point(163,117);
                this.lblHintTextColor.Location=new Point(165,142);
                this.lblBackgroundColor.Location=new Point(145,167);

                this.picGhost.Location=new Point(325,200);

                this.rtbInfo.Location=new Point(13,50);

                this.tbKeyID.Location=new Point(14,90);
                this.tbTimerID.Location=new Point(85,90);
                this.tbTextColor_rgb.Location=new Point(357,65);
                this.tbBoxForeColor_rgb.Location=new Point(357,90);
                this.tbBoxBackColor_rgb.Location=new Point(357,115);
                this.tbHintTextColor_rgb.Location=new Point(357,140);
                this.tbBackgroundColor_rgb.Location=new Point(357,165);
                this.tbTextColorR.Location=new Point(267,65);
                this.tbTextColorG.Location=new Point(297,65);
                this.tbTextColorB.Location=new Point(327,65);
                this.tbBoxForeColorR.Location=new Point(267,90);
                this.tbBoxForeColorG.Location=new Point(297,90);
                this.tbBoxForeColorB.Location=new Point(327,90);
                this.tbBoxBackColorR.Location=new Point(267,115);
                this.tbBoxBackColorG.Location=new Point(297,115);
                this.tbBoxBackColorB.Location=new Point(327,115);
                this.tbHintTextColorR.Location=new Point(267,140);
                this.tbHintTextColorG.Location=new Point(297,140);
                this.tbHintTextColorB.Location=new Point(327,140);
                this.tbBackgroundColorR.Location=new Point(267,165);
                this.tbBackgroundColorG.Location=new Point(297,165);
                this.tbBackgroundColorB.Location=new Point(327,165);

                this.trbOpacity.Location=new Point(6,205);
                this.trbSize.Location=new Point(6,145);
            }
            if((int)Settings.Default["Size"]==2) {
                this.btnHintClose.Location=new Point(398,7);
                this.btnSetClose.Location=new Point(398,7);
                this.btnResetColor.Location=new Point(311,222);
                this.btnOK.Location=new Point(366,7);

                this.chbCompact.Location=new Point(125,284);
                this.chbSoundOnOff.Location=new Point(16,284);

                this.cbLanguage.Location=new Point(17,41);
                this.cbInfo.Location=new Point(17,17);

                this.lblHotKey.Location=new Point(12,81);
                this.lblTimerKey.Location=new Point(97,81);
                this.lblLanguage.Location=new Point(12,19);
                this.lblOpacity.Location=new Point(12,216);
                this.lblOpacity_Value.Location=new Point(74,216);
                this.lblSize.Location=new Point(12,146);
                this.lblSize_Value.Location=new Point(51,146);
                this.lblCopyright.Location=new Point(319,301);
                this.lblVersion.Location=new Point(329,270);
                this.lblTextColor.Location=new Point(224,80);
                this.lblBoxForeColor.Location=new Point(190,107);
                this.lblBoxBackColor.Location=new Point(187,136);
                this.lblHintTextColor.Location=new Point(191,164);
                this.lblBackgroundColor.Location=new Point(168,193);

                this.picGhost.Location=new Point(374,231);
                
                this.rtbInfo.Location=new Point(15,58);

                this.tbKeyID.Location=new Point(16,105);
                this.tbTimerID.Location=new Point(99,105);
                this.tbTextColor_rgb.Location=new Point(411,77);
                this.tbBoxForeColor_rgb.Location=new Point(411,105);
                this.tbBoxBackColor_rgb.Location=new Point(411,134);
                this.tbHintTextColor_rgb.Location=new Point(411,163);
                this.tbBackgroundColor_rgb.Location=new Point(411,192);
                this.tbTextColorR.Location=new Point(308,77);
                this.tbTextColorG.Location=new Point(342,77);
                this.tbTextColorB.Location=new Point(376,77);
                this.tbBoxForeColorR.Location=new Point(308,105);
                this.tbBoxForeColorG.Location=new Point(342,105);
                this.tbBoxForeColorB.Location=new Point(376,105);
                this.tbBoxBackColorR.Location=new Point(308,134);
                this.tbBoxBackColorG.Location=new Point(342,134);
                this.tbBoxBackColorB.Location=new Point(376,134);
                this.tbHintTextColorR.Location=new Point(308,163);
                this.tbHintTextColorG.Location=new Point(342,163);
                this.tbHintTextColorB.Location=new Point(376,163);
                this.tbBackgroundColorR.Location=new Point(308,192);
                this.tbBackgroundColorG.Location=new Point(342,192);
                this.tbBackgroundColorB.Location=new Point(376,192);

                this.trbOpacity.Location=new Point(8,238);
                this.trbSize.Location=new Point(8,168);
            }
            if((int)Settings.Default["Size"]==3) {
                this.btnHintClose.Location=new Point(433,7);
                this.btnSetClose.Location=new Point(433,7);
                this.btnResetColor.Location=new Point(338,240);
                this.btnOK.Location=new Point(397,7);

                this.chbCompact.Location=new Point(136,309);
                this.chbSoundOnOff.Location=new Point(19,309);

                this.cbLanguage.Location=new Point(18,45);
                this.cbInfo.Location=new Point(18,18);

                this.lblHotKey.Location=new Point(13,88);
                this.lblTimerKey.Location=new Point(104,88);
                this.lblLanguage.Location=new Point(13,19);
                this.lblOpacity.Location=new Point(13,234);
                this.lblOpacity_Value.Location=new Point(80,234);
                this.lblSize.Location=new Point(13,158);
                this.lblSize_Value.Location=new Point(55,158);
                this.lblCopyright.Location=new Point(345,329);
                this.lblVersion.Location=new Point(357,294);
                this.lblTextColor.Location=new Point(239,85);
                this.lblBoxForeColor.Location=new Point(203,115);
                this.lblBoxBackColor.Location=new Point(200,147);
                this.lblHintTextColor.Location=new Point(203,178);
                this.lblBackgroundColor.Location=new Point(178,209);

                this.picGhost.Location=new Point(407,250);
                
                this.rtbInfo.Location=new Point(16,63);

                this.tbKeyID.Location=new Point(18,114);
                this.tbTimerID.Location=new Point(107,114);
                this.tbTextColor_rgb.Location=new Point(447,83);
                this.tbBoxForeColor_rgb.Location=new Point(447,114);
                this.tbBoxBackColor_rgb.Location=new Point(447,146);
                this.tbHintTextColor_rgb.Location=new Point(447,177);
                this.tbBackgroundColor_rgb.Location=new Point(447,209);
                this.tbTextColorR.Location=new Point(334,83);
                this.tbTextColorG.Location=new Point(372,83);
                this.tbTextColorB.Location=new Point(409,83);
                this.tbBoxForeColorR.Location=new Point(334,114);
                this.tbBoxForeColorG.Location=new Point(372,114);
                this.tbBoxForeColorB.Location=new Point(409,114);
                this.tbBoxBackColorR.Location=new Point(334,146);
                this.tbBoxBackColorG.Location=new Point(372,146);
                this.tbBoxBackColorB.Location=new Point(409,146);
                this.tbHintTextColorR.Location=new Point(334,177);
                this.tbHintTextColorG.Location=new Point(372,177);
                this.tbHintTextColorB.Location=new Point(409,177);
                this.tbBackgroundColorR.Location=new Point(334,209);
                this.tbBackgroundColorG.Location=new Point(372,209);
                this.tbBackgroundColorB.Location=new Point(409,209);

                this.trbOpacity.Location=new Point(8,260);
                this.trbSize.Location=new Point(8,184);
            }
            if((int)Settings.Default["Size"]==4) {
                this.btnHintClose.Location=new Point(462,7);
                this.btnSetClose.Location=new Point(462,7);
                this.btnResetColor.Location=new Point(361,257);
                this.btnOK.Location=new Point(425,7);

                this.chbCompact.Location=new Point(144,329);
                this.chbSoundOnOff.Location=new Point(19,329);

                this.cbLanguage.Location=new Point(20,49);
                this.cbInfo.Location=new Point(20,20);

                this.lblHotKey.Location=new Point(14,94);
                this.lblTimerKey.Location=new Point(111,94);
                this.lblLanguage.Location=new Point(14,21);
                this.lblOpacity.Location=new Point(14,250);
                this.lblOpacity_Value.Location=new Point(86,250);
                this.lblSize.Location=new Point(14,169);
                this.lblSize_Value.Location=new Point(59,169);
                this.lblCopyright.Location=new Point(369,351);
                this.lblVersion.Location=new Point(381,314);
                this.lblTextColor.Location=new Point(259,91);
                this.lblBoxForeColor.Location=new Point(219,123);
                this.lblBoxBackColor.Location=new Point(218,157);
                this.lblHintTextColor.Location=new Point(221,190);
                this.lblBackgroundColor.Location=new Point(194,223);

                this.picGhost.Location=new Point(434,267);
                
                this.rtbInfo.Location=new Point(17,67);

                this.tbKeyID.Location=new Point(19,121);
                this.tbTimerID.Location=new Point(114,121);
                this.tbTextColor_rgb.Location=new Point(477,88);
                this.tbBoxForeColor_rgb.Location=new Point(477,122);
                this.tbBoxBackColor_rgb.Location=new Point(477,155);
                this.tbHintTextColor_rgb.Location=new Point(477,188);
                this.tbBackgroundColor_rgb.Location=new Point(477,222);
                this.tbTextColorR.Location=new Point(357,87);
                this.tbTextColorG.Location=new Point(397,87);
                this.tbTextColorB.Location=new Point(437,87);
                this.tbBoxForeColorR.Location=new Point(357,121);
                this.tbBoxForeColorG.Location=new Point(397,121);
                this.tbBoxForeColorB.Location=new Point(437,121);
                this.tbBoxBackColorR.Location=new Point(357,154);
                this.tbBoxBackColorG.Location=new Point(397,154);
                this.tbBoxBackColorB.Location=new Point(437,154);
                this.tbHintTextColorR.Location=new Point(357,187);
                this.tbHintTextColorG.Location=new Point(397,187);
                this.tbHintTextColorB.Location=new Point(437,187);
                this.tbBackgroundColorR.Location=new Point(357,221);
                this.tbBackgroundColorG.Location=new Point(397,221);
                this.tbBackgroundColorB.Location=new Point(437,221);

                this.trbOpacity.Location=new Point(10,277);
                this.trbSize.Location=new Point(10,197);
            }
        }
        private void set_lblCounter_lblDif_size() {
            if((int)Settings.Default["Size"]==1) {
                this.lblDif.Height=21;
                this.lblDif.Width=21;

                this.tbCounter.Height=21;
                this.tbCounter.Width=39;
                this.tbTextColor_rgb.Height=21;
                this.tbBoxForeColor_rgb.Height = 21;
                this.tbBoxBackColor_rgb.Height = 21;
                this.tbHintTextColor_rgb.Height = 21;
                this.tbBackgroundColor_rgb.Height = 21;
            }
            if((int)Settings.Default["Size"]==2) {
                this.lblDif.Height=23;
                this.lblDif.Width=23;

                this.tbCounter.Height=23;
                this.tbCounter.Width=44;
                this.tbTextColor_rgb.Height=22;
                this.tbBoxForeColor_rgb.Height = 22;
                this.tbBoxBackColor_rgb.Height = 22;
                this.tbHintTextColor_rgb.Height = 22;
                this.tbBackgroundColor_rgb.Height = 22;
            }
            if((int)Settings.Default["Size"]==3) {
                this.lblDif.Height=26;
                this.lblDif.Width=26;

                this.tbCounter.Height=26;
                this.tbCounter.Width=47;
                this.tbTextColor_rgb.Height=23;
                this.tbBoxForeColor_rgb.Height = 23;
                this.tbBoxBackColor_rgb.Height = 23;
                this.tbHintTextColor_rgb.Height = 23;
                this.tbBackgroundColor_rgb.Height = 23;
            }
            if((int)Settings.Default["Size"]==4) {
                this.lblDif.Height=28;
                this.lblDif.Width=28;

                this.tbCounter.Height=28;
                this.tbCounter.Width=52;
                this.tbTextColor_rgb.Height=24;
                this.tbBoxForeColor_rgb.Height = 24;
                this.tbBoxBackColor_rgb.Height = 24;
                this.tbHintTextColor_rgb.Height = 24;
                this.tbBackgroundColor_rgb.Height = 24;
            }
        }
        private void set_font_size() {
            if((int)Settings.Default["Size"]==1) {
                this.cbFirstName.Font=new Font(FontFamily.GenericSansSerif,9);
                this.cbLastName.Font=new Font(FontFamily.GenericSansSerif,9);
                this.cbAnswerType.Font=new Font(FontFamily.GenericSansSerif,8);
                this.cbTask1.Font=new Font(FontFamily.GenericSansSerif,9);
                this.cbTask2.Font=new Font(FontFamily.GenericSansSerif,9);
                this.cbTask3.Font=new Font(FontFamily.GenericSansSerif,9);
                this.cbGhosts.Font=new Font(FontFamily.GenericSansSerif,9);
                this.cbTimerType.Font=new Font(FontFamily.GenericSansSerif,8);
                this.cbMaps.Font=new Font(FontFamily.GenericSansSerif,8);

                this.lblDif.Font=new Font(FontFamily.GenericSansSerif,11);
                this.lblHint.Font=new Font(FontFamily.GenericSansSerif,9);
                this.lblGhInfoType.Font=new Font(FontFamily.GenericSansSerif,10);
                this.lblGhInfoTypeData.Font=new Font(FontFamily.GenericSansSerif,10);
                this.lblGhInfoHunt.Font=new Font(FontFamily.GenericSansSerif,10);
                this.lblGhInfoHuntData.Font=new Font(FontFamily.GenericSansSerif,10);
                this.lblGhInfoHuntAbilities.Font=new Font(FontFamily.GenericSansSerif,10);
                this.lblGhInfoHuntAbilitiesData.Font=new Font(FontFamily.GenericSansSerif,10);
                this.lblGhInfoCooldown.Font=new Font(FontFamily.GenericSansSerif,10);
                this.lblGhInfoCooldownData.Font=new Font(FontFamily.GenericSansSerif,10);
                
                this.rtbGhostTyp.Font=new Font(FontFamily.GenericSansSerif,10);
                this.rtbGhInfoHintsData.Font=new Font(FontFamily.GenericSansSerif,10);

                this.tbCounter.Font=new Font(FontFamily.GenericSansSerif,9);
            }
            if((int)Settings.Default["Size"]==2) {
                this.cbFirstName.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.cbLastName.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.cbAnswerType.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(9));
                this.cbTask1.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.cbTask2.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.cbTask3.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.cbGhosts.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.cbTimerType.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(9));
                this.cbMaps.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(9));
                
                this.lblDif.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.lblHint.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.lblGhInfoType.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.lblGhInfoTypeData.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.lblGhInfoHunt.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.lblGhInfoHuntData.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.lblGhInfoHuntAbilities.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.lblGhInfoHuntAbilitiesData.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.lblGhInfoCooldown.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.lblGhInfoCooldownData.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                
                this.rtbGhostTyp.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.rtbGhInfoHintsData.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));

                this.tbCounter.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(11));
            }
            if((int)Settings.Default["Size"]==3) {
                this.cbFirstName.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.cbLastName.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.cbAnswerType.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(11));
                this.cbTask1.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.cbTask2.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.cbTask3.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.cbGhosts.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.cbTimerType.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(11));
                this.cbMaps.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(11));

                this.lblDif.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(14));
                this.lblHint.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.lblGhInfoType.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblGhInfoTypeData.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblGhInfoHunt.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblGhInfoHuntData.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblGhInfoHuntAbilities.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblGhInfoHuntAbilitiesData.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblGhInfoCooldown.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblGhInfoCooldownData.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                
                this.rtbGhostTyp.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.rtbGhInfoHintsData.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));

                this.tbCounter.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(14));
            }
            if((int)Settings.Default["Size"]==4) {
                this.cbFirstName.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(14));
                this.cbLastName.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(14));
                this.cbAnswerType.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.cbTask1.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(14));
                this.cbTask2.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(14));
                this.cbTask3.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(14));
                this.cbGhosts.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(14));
                this.cbTimerType.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.cbMaps.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));

                this.lblDif.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(14));
                this.lblHint.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(14));
                this.lblGhInfoType.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblGhInfoTypeData.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblGhInfoHunt.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblGhInfoHuntData.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblGhInfoHuntAbilities.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblGhInfoHuntAbilitiesData.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblGhInfoCooldown.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblGhInfoCooldownData.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                
                this.rtbGhostTyp.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.rtbGhInfoHintsData.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));

                this.tbCounter.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(15));
            }
        }
        private void set_font_size_options_panel() {
            if((int)Settings.Default["Size"]==1) {
                this.chbCompact.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.chbSoundOnOff.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                
                this.cbLanguage.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(9));

                this.lblHotKey.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.lblTimerKey.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.lblLanguage.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.lblOpacity.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.lblOpacity_Value.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.lblSize.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.lblSize_Value.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.lblCopyright.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(8));
                this.lblVersion.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(8));
                this.lblTextColor.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.lblBoxForeColor.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.lblBoxBackColor.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.lblHintTextColor.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.lblBackgroundColor.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));

                this.tbKeyID.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(9));
                this.tbTimerID.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(9));
                this.tbTextColorR.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(9));
                this.tbTextColorG.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(9));
                this.tbTextColorB.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(9));
                this.tbBoxForeColorR.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(9));
                this.tbBoxForeColorG.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(9));
                this.tbBoxForeColorB.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(9));
                this.tbBoxBackColorR.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(9));
                this.tbBoxBackColorG.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(9));
                this.tbBoxBackColorB.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(9));
                this.tbHintTextColorR.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(9));
                this.tbHintTextColorG.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(9));
                this.tbHintTextColorB.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(9));
                this.tbBackgroundColorR.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(9));
                this.tbBackgroundColorG.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(9));
                this.tbBackgroundColorB.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(9));
            }
            if((int)Settings.Default["Size"]==2) {
                this.chbSoundOnOff.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.chbCompact.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                
                this.cbLanguage.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(11));

                this.lblHotKey.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.lblTimerKey.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.lblLanguage.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.lblOpacity.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.lblOpacity_Value.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.lblSize.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.lblSize_Value.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.lblCopyright.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(9));
                this.lblVersion.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(9));
                this.lblTextColor.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.lblBoxForeColor.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.lblBoxBackColor.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.lblHintTextColor.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.lblBackgroundColor.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                
                this.tbKeyID.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.tbTimerID.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.tbTextColorR.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.tbTextColorG.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.tbTextColorB.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.tbBoxForeColorR.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.tbBoxForeColorG.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.tbBoxForeColorB.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.tbBoxBackColorR.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.tbBoxBackColorG.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.tbBoxBackColorB.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.tbHintTextColorR.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.tbHintTextColorG.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.tbHintTextColorB.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.tbBackgroundColorR.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.tbBackgroundColorG.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.tbBackgroundColorB.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
            }
            if((int)Settings.Default["Size"]==3) {
                this.chbSoundOnOff.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.chbCompact.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                
                this.cbLanguage.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));

                this.lblHotKey.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblTimerKey.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblLanguage.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblOpacity.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblOpacity_Value.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblSize.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblSize_Value.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblCopyright.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.lblVersion.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.lblTextColor.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblBoxForeColor.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblBoxBackColor.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblHintTextColor.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.lblBackgroundColor.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                
                this.tbKeyID.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(11));
                this.tbTimerID.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(11));
                this.tbTextColorR.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(11));
                this.tbTextColorG.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(11));
                this.tbTextColorB.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(11));
                this.tbBoxForeColorR.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(11));
                this.tbBoxForeColorG.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(11));
                this.tbBoxForeColorB.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(11));
                this.tbBoxBackColorR.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(11));
                this.tbBoxBackColorG.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(11));
                this.tbBoxBackColorB.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(11));
                this.tbHintTextColorR.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(11));
                this.tbHintTextColorG.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(11));
                this.tbHintTextColorB.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(11));
                this.tbBackgroundColorR.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(11));
                this.tbBackgroundColorG.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(11));
                this.tbBackgroundColorB.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(11));
            }
            if((int)Settings.Default["Size"]==4) {
                this.chbSoundOnOff.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(14));
                this.chbCompact.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(14));
                
                this.cbLanguage.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));

                this.lblHotKey.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(14));
                this.lblTimerKey.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(14));
                this.lblLanguage.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(14));
                this.lblOpacity.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(14));
                this.lblOpacity_Value.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(14));
                this.lblSize.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(14));
                this.lblSize_Value.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(14));
                this.lblCopyright.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.lblVersion.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.lblTextColor.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(14));
                this.lblBoxForeColor.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(14));
                this.lblBoxBackColor.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(14));
                this.lblHintTextColor.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(14));
                this.lblBackgroundColor.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(14));
                
                this.tbKeyID.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.tbTimerID.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.tbTextColorR.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.tbTextColorG.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.tbTextColorB.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.tbBoxForeColorR.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.tbBoxForeColorG.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.tbBoxForeColorB.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.tbBoxBackColorR.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.tbBoxBackColorG.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.tbBoxBackColorB.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.tbHintTextColorR.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.tbHintTextColorG.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.tbHintTextColorB.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.tbBackgroundColorR.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.tbBackgroundColorG.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.tbBackgroundColorB.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
            }
        }
        private void set_font_size_help_panel() {
            if((int)Settings.Default["Size"]==1) {
                this.cbInfo.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
                this.rtbInfo.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(10));
            }
            if((int)Settings.Default["Size"]==2) {
                this.cbInfo.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
                this.rtbInfo.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(12));
            }
            if((int)Settings.Default["Size"]==3) {
                this.cbInfo.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.rtbInfo.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
            }
            if((int)Settings.Default["Size"]==4) {
                this.cbInfo.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
                this.rtbInfo.Font=new Font(FontFamily.GenericSansSerif,Convert.ToSingle(13));
            }
        }
        private void SetBackgroundSettings() {
            this.lblTextColor.Visible=true;
            this.lblBoxForeColor.Visible=true;
            this.lblBoxBackColor.Visible=true;
            this.lblHintTextColor.Visible=true;
            this.lblBackgroundColor.Visible=true;
            
            this.tbTextColor_rgb.Visible=true;
            this.tbBoxForeColor_rgb.Visible=true;
            this.tbBoxBackColor_rgb.Visible=true;
            this.tbHintTextColor_rgb.Visible=true;
            this.tbBackgroundColor_rgb.Visible=true;

            this.tbTextColorR.Visible=true;
            this.tbTextColorG.Visible=true;
            this.tbTextColorB.Visible=true;
            this.tbBoxForeColorR.Visible=true;
            this.tbBoxForeColorG.Visible=true;
            this.tbBoxForeColorB.Visible=true;
            this.tbBoxBackColorR.Visible=true;
            this.tbBoxBackColorG.Visible=true;
            this.tbBoxBackColorB.Visible=true;
            this.tbHintTextColorR.Visible=true;
            this.tbHintTextColorG.Visible=true;
            this.tbHintTextColorB.Visible=true;
            this.tbBackgroundColorR.Visible=true;
            this.tbBackgroundColorG.Visible=true;
            this.tbBackgroundColorB.Visible=true;

            this.tbTextColorR.MaxLength=3;
            this.tbTextColorG.MaxLength=3;
            this.tbTextColorB.MaxLength=3;
            this.tbBoxForeColorR.MaxLength=3;
            this.tbBoxForeColorG.MaxLength=3;
            this.tbBoxForeColorB.MaxLength=3;
            this.tbBoxBackColorR.MaxLength=3;
            this.tbBoxBackColorG.MaxLength=3;
            this.tbBoxBackColorB.MaxLength=3;
        }

        private void init_wavs() {
            string str="beep1.wav";
            this.beep_1=!File.Exists(str)?new SoundPlayer((Stream)Resources.beep1):new SoundPlayer(str);
            str="beep2.wav";
            this.beep_2=!File.Exists(str)?new SoundPlayer((Stream)Resources.beep2):new SoundPlayer(str);
            str="beep5.wav";
            this.beep_3=!File.Exists(str)?new SoundPlayer((Stream)Resources.beep3):new SoundPlayer(str);
            str="beep_finish.wav";
            this.beep_finish=!File.Exists(str)?new SoundPlayer((Stream)Resources.beep_finish):new SoundPlayer(str);
        }
        private void init_icons() {
            string str="Images/EMF.png";
            this.EMFReaderIcon48=!File.Exists(str)?new Bitmap((Image)Resources.EMFReaderIcon48):new Bitmap(str);
            str="Images/EMF_green.png";
            this.EMFReaderIcon48g=!File.Exists(str)?new Bitmap((Image)Resources.EMFReaderIcon48g):new Bitmap(str);
            str="Images/EMF_red.png";
            this.EMFReaderIcon48r=!File.Exists(str)?new Bitmap((Image)Resources.EMFReaderIcon48r):new Bitmap(str);
            str="Images/EMF_red_hold.png";
            this.EMFReaderIcon48y=!File.Exists(str)?new Bitmap((Image)Resources.EMFReaderIcon48y):new Bitmap(str);
            
            str="Images/Finger.png";
            this.FingerprintIcon48=!File.Exists(str)?new Bitmap((Image)Resources.FingerprintIcon48):new Bitmap(str);
            str="Images/Finger_green.png";
            this.FingerprintIcon48g=!File.Exists(str)?new Bitmap((Image)Resources.FingerprintIcon48g):new Bitmap(str);
            str="Images/Finger_red.png";
            this.FingerprintIcon48r=!File.Exists(str)?new Bitmap((Image)Resources.FingerprintIcon48r):new Bitmap(str);
            str="Images/Finger_red_hold.png";
            this.FingerprintIcon48y=!File.Exists(str)?new Bitmap((Image)Resources.FingerprintIcon48y):new Bitmap(str);
            
            str="Images/Book.png";
            this.BookIcon48=!File.Exists(str)?new Bitmap((Image)Resources.BookIcon48):new Bitmap(str);
            str="Images/Book_green.png";
            this.BookIcon48g=!File.Exists(str)?new Bitmap((Image)Resources.BookIcon48g):new Bitmap(str);
            str="Images/Book_red.png";
            this.BookIcon48r=!File.Exists(str)?new Bitmap((Image)Resources.BookIcon48r):new Bitmap(str);
            str="Images/Book_red_hold.png";
            this.BookIcon48y=!File.Exists(str)?new Bitmap((Image)Resources.BookIcon48y):new Bitmap(str);
            
            str="Images/Temp.png";
            this.BreathingIcon48=!File.Exists(str)?new Bitmap((Image)Resources.BreathingIcon48):new Bitmap(str);
            str="Images/Temp_green.png";
            this.BreathingIcon48g=!File.Exists(str)?new Bitmap((Image)Resources.BreathingIcon48g):new Bitmap(str);
            str="Images/Temp_red.png";
            this.BreathingIcon48r=!File.Exists(str)?new Bitmap((Image)Resources.BreathingIcon48r):new Bitmap(str);
            str="Images/Temp_red_hold.png";
            this.BreathingIcon48y=!File.Exists(str)?new Bitmap((Image)Resources.BreathingIcon48y):new Bitmap(str);
            
            str="Images/Orb.png";
            this.GhostOrbIcon48=!File.Exists(str)?new Bitmap((Image)Resources.GhostOrbIcon48):new Bitmap(str);
            str="Images/Orb_green.png";
            this.GhostOrbIcon48g=!File.Exists(str)?new Bitmap((Image)Resources.GhostOrbIcon48g):new Bitmap(str);
            str="Images/Orb_red.png";
            this.GhostOrbIcon48r=!File.Exists(str)?new Bitmap((Image)Resources.GhostOrbIcon48r):new Bitmap(str);
            str="Images/Orb_red_hold.png";
            this.GhostOrbIcon48y=!File.Exists(str)?new Bitmap((Image)Resources.GhostOrbIcon48y):new Bitmap(str);
            
            str="Images/Box.png";
            this.SpiritBoxIcon48=!File.Exists(str)?new Bitmap((Image)Resources.SpiritBoxIcon48):new Bitmap(str);
            str="Images/Box_green.png";
            this.SpiritBoxIcon48g=!File.Exists(str)?new Bitmap((Image)Resources.SpiritBoxIcon48g):new Bitmap(str);
            str="Images/Box_red.png";
            this.SpiritBoxIcon48r=!File.Exists(str)?new Bitmap((Image)Resources.SpiritBoxIcon48r):new Bitmap(str);
            str="Images/Box_red_hold.png";
            this.SpiritBoxIcon48y=!File.Exists(str)?new Bitmap((Image)Resources.SpiritBoxIcon48y):new Bitmap(str);
            
            str="Images/Dots.png";
            this.DotsIcon48=!File.Exists(str)?new Bitmap((Image)Resources.DotsIcon48):new Bitmap(str);
            str="Images/Dots_green.png";
            this.DotsIcon48g=!File.Exists(str)?new Bitmap((Image)Resources.DotsIcon48g):new Bitmap(str);
            str="Images/Dots_red.png";
            this.DotsIcon48r=!File.Exists(str)?new Bitmap((Image)Resources.DotsIcon48r):new Bitmap(str);
            str="Images/Dots_red_hold.png";
            this.DotsIcon48y=!File.Exists(str)?new Bitmap((Image)Resources.DotsIcon48y):new Bitmap(str);
            
            str="Images/BN_Reset.png";
            this.ResetIcon=!File.Exists(str)?new Bitmap((Image)Resources.ResetIcon):new Bitmap(str);
            str="Images/BN_Settings.png";
            this.SettingsIcon=!File.Exists(str)?new Bitmap((Image)Resources.SettingsIcon):new Bitmap(str);
            str="Images/BN_Min.png";
            this.btnMinIcon=!File.Exists(str)?new Bitmap((Image)Resources.MinIcon48):new Bitmap(str);
            str="Images/BN_Exit.png";
            this.btnExitIcon=!File.Exists(str)?new Bitmap((Image)Resources.ExitIcon48):new Bitmap(str);
            str="Images/BN_Arrow_down.png";
            this.arrow_down=!File.Exists(str)?new Bitmap((Image)Resources.arrow_down):new Bitmap(str);
            str="Images/BN_Arrow_up.png";
            this.arrow_up=!File.Exists(str)?new Bitmap((Image)Resources.arrow_up):new Bitmap(str);
            str="Images/BN_Line.png";
            this.LineIcon=!File.Exists(str)?new Bitmap((Image)Resources.line):new Bitmap(str);
            str="Images/BN_OK.png";
            this.OkIcon=!File.Exists(str)?new Bitmap((Image)Resources.OKIcon48):new Bitmap(str);
            str="Images/BN_Hekp.png";
            this.HelpIcon=!File.Exists(str)?new Bitmap((Image)Resources.HelpIcon):new Bitmap(str);

            //Pictures
            this.picGhost.BackgroundImage=(Image)new Bitmap((Image)Resources.ghost_symbol_x32png);
        }
        public Bitmap ResizeBitmap(Bitmap bmp,int width,int height) {
            Bitmap bitmap=new Bitmap(width,height);
            using(Graphics graphics=Graphics.FromImage((Image)bitmap))
                graphics.DrawImage((Image)bmp,0,0,width,height);
            return bitmap;
        }
        private void setting_language() {
            if((int)Settings.Default["Lang"]==0)
                this.var_language="en";
            if((int)Settings.Default["Lang"]>=1)
                this.var_language="ru";
        }
        private void load_files_once() {
            try {
                this.cbFirstName.Items.AddRange((object[])File.ReadAllLines("Data/First_Names.txt"));
                this.cbLastName.Items.AddRange((object[])File.ReadAllLines("Data/Last_Names.txt"));
                this.var_hint=File.ReadAllText("Data/"+this.var_language+"/Hint.txt");
            } catch {
                if(this.cbFirstName.Items.Count==0)
                    this.cbFirstName.Items.Add((object)"");
                if(this.cbLastName.Items.Count==0)
                    this.cbLastName.Items.Add((object)"");
            }
        }
        private void load_files() {
            this.GhostList.Clear();
            this.TimerList.Clear();
            this.TableList.Clear();

            try {
                this.GhostList=new JavaScriptSerializer().Deserialize<List<Helper_Ghosts>>(File.ReadAllText("Data/"+this.var_language+"/Ghosts.txt"));
                this.TimerList=new JavaScriptSerializer().Deserialize<List<Helper_Timer>>(File.ReadAllText("Data/"+this.var_language+"/Timer.txt"));
                this.TableList=new JavaScriptSerializer().Deserialize<List<Helper_Tables>>(File.ReadAllText("Data/"+this.var_language+"/Help.txt"));
            } catch {
                if(this.GhostList.Count==0)
                    this.GhostList.Add(new Helper_Ghosts() {
                        Ghostname="Error",
                        Evidence1=" ",
                        Evidence2=" ",
                        Evidence3=" ",
                        Evidence4=" ",
                        Evidence_Guaranteed=" ",
                        Type_Title=" ",
                        Type=" ",
                        Hunt_Title=" ",
                        Hunt=" ",
                        Hunt_Abilities_Title=" ",
                        Hunt_Abilities=" ",
                        Smudging_Title=" ",
                        Smudging=" ",
                        Hints=new List<string> {""}
                    });
                if(this.TimerList.Count==0)
                    this.TimerList.Add(new Helper_Timer() {
                        Timer_Hunt=" ",
                        Timer_Hunt_Cursed=" ",
                        Timer_Hunt_Post=" ",
                        Timer_Hunt_Post_Demon=" ",
                        Timer_Hunt_Smudge=" ",
                        Timer_Smudge_Demon=" ",
                        Timer_Smudge_Spirit=" ",
                        Timer_Start=" ",
                        Dif_Amateur=" ",
                        Dif_Intermediate=" ",
                        Dif_Nightmare=" ",
                        Dif_Professional=" ",
                        Small=" ",
                        Medium=" ",
                        Big=" "
                    });
                if(this.TableList.Count==0)
                    this.TableList.Add(new Helper_Tables() {
                        Title=" ",
                        Text=new List<string> {""}
                    });
            }
        }

        private void check_ghosttyp() {
            this.rtbGhostTyp.Text=" ";
            this.var_countGhosts=0;
            this.EviListRed.Clear();
            this.EviListRed=this.EviListAll.ToList<string>();

            foreach(Helper_Ghosts ghost in this.GhostList) {
                int num=0;
                foreach(string str in this.EviListGreen) {
                    if(str==ghost.Evidence1 || str==ghost.Evidence2 || str==ghost.Evidence3 || str==ghost.Evidence4)
                        ++num;
                }
                foreach(string str in this.EviListHold) {
                    if(str==ghost.Evidence1 || str==ghost.Evidence2 || str==ghost.Evidence3 || str==ghost.Evidence4)
                        --num;
                }
                if(num==this.EviListGreen.Count) {
                    this.rtbGhostTyp.Text=this.rtbGhostTyp.Text+ghost.Ghostname+", ";
                    ++this.var_countGhosts;
                    if(this.EviListGreen.Count!=0) {
                        foreach(string str in this.EviListAll) {
                            if((str==ghost.Evidence1 || str==ghost.Evidence2 || str==ghost.Evidence3 || str==ghost.Evidence4) && this.EviListRed.Contains(str))
                                this.EviListRed.Remove(str);
                        }
                    } else {
                        this.EviListRed.Clear();
                    }
                }
            }

            if(this.rtbGhostTyp.Text.Length>1)
                this.rtbGhostTyp.Text=string.Join(",",((IEnumerable<string>)this.rtbGhostTyp.Text.Split(',')).ToList<string>().OrderBy<string,string>((Func<string,string>)(o => o)).ToArray<string>()).Remove(0,3);
            if(this.var_countGhosts==1 &&(int)Settings.Default["CompactMode"]==0) {
                this.var_countGhosts=0;
                this.lblHint.Text=this.var_hint;
                this.lblHint.Visible=true;
            } else {
                this.lblHint.Text="";
                this.lblHint.Visible=false;
            }
            this.set_icons();
        }
        private void set_icons() {
            if(this.EviListGreen.Contains("EMF"))
                this.btnEMF.BackgroundImage=(Image)this.EMFReaderIcon48g;
            else if(this.EviListHold.Contains("EMF"))
                this.btnEMF.BackgroundImage=(Image)this.EMFReaderIcon48y;
            else if(this.EviListRed.Contains("EMF"))
                this.btnEMF.BackgroundImage=(Image)this.EMFReaderIcon48r;
            else
                this.btnEMF.BackgroundImage=(Image)this.EMFReaderIcon48;
            
            if(this.EviListGreen.Contains("Finger"))
                this.btnFinger.BackgroundImage=(Image)this.FingerprintIcon48g;
            else if(this.EviListHold.Contains("Finger"))
                this.btnFinger.BackgroundImage=(Image)this.FingerprintIcon48y;
            else if(this.EviListRed.Contains("Finger"))
                this.btnFinger.BackgroundImage=(Image)this.FingerprintIcon48r;
            else
                this.btnFinger.BackgroundImage=(Image)this.FingerprintIcon48;
            
            if(this.EviListGreen.Contains("Book"))
                this.btnBook.BackgroundImage=(Image)this.BookIcon48g;
            else if(this.EviListHold.Contains("Book"))
                this.btnBook.BackgroundImage=(Image)this.BookIcon48y;
            else if(this.EviListRed.Contains("Book"))
                this.btnBook.BackgroundImage=(Image)this.BookIcon48r;
            else
                this.btnBook.BackgroundImage=(Image)this.BookIcon48;

            if(this.EviListGreen.Contains("Temp"))
                this.btnTemp.BackgroundImage=(Image)this.BreathingIcon48g;
            else if(this.EviListHold.Contains("Temp"))
                this.btnTemp.BackgroundImage=(Image)this.BreathingIcon48y;
            else if(this.EviListRed.Contains("Temp"))
                this.btnTemp.BackgroundImage=(Image)this.BreathingIcon48r;
            else
                this.btnTemp.BackgroundImage=(Image)this.BreathingIcon48;

            if(this.EviListGreen.Contains("Orb"))
                this.btnOrb.BackgroundImage=(Image)this.GhostOrbIcon48g;
            else if(this.EviListHold.Contains("Orb"))
                this.btnOrb.BackgroundImage=(Image)this.GhostOrbIcon48y;
            else if(this.EviListRed.Contains("Orb"))
                this.btnOrb.BackgroundImage=(Image)this.GhostOrbIcon48r;
            else
                this.btnOrb.BackgroundImage=(Image)this.GhostOrbIcon48;
            
            if(this.EviListGreen.Contains("Box"))
                this.btnBox.BackgroundImage=(Image)this.SpiritBoxIcon48g;
            else if(this.EviListHold.Contains("Box"))
                this.btnBox.BackgroundImage=(Image)this.SpiritBoxIcon48y;
            else if(this.EviListRed.Contains("Box"))
                this.btnBox.BackgroundImage=(Image)this.SpiritBoxIcon48r;
            else
                this.btnBox.BackgroundImage=(Image)this.SpiritBoxIcon48;

            if(this.EviListGreen.Contains("Dots"))
                this.btnDots.BackgroundImage=(Image)this.DotsIcon48g;
            else if(this.EviListHold.Contains("Dots"))
                this.btnDots.BackgroundImage=(Image)this.DotsIcon48y;
            else if(this.EviListRed.Contains("Dots"))
                this.btnDots.BackgroundImage=(Image)this.DotsIcon48r;
            else
                this.btnDots.BackgroundImage=(Image)this.DotsIcon48;
        }
        private void UserIconImages() {
            string str="Images/BN_OK.png";
            if(File.Exists(str))
                this.OkIcon=new Bitmap(str);
            str="Images/BN_Exit.png";
            if(File.Exists(str))
                this.OkIcon=new Bitmap(str);
        }
        private Bitmap getPic(string name,int color) {
            if(color==0) {
                switch(name) {
                    case "EMF":
                        return this.EMFReaderIcon48;
                    case "Finger":
                        return this.FingerprintIcon48;
                    case "Book":
                        return this.BookIcon48;
                    case "Temp":
                        return this.BreathingIcon48;
                    case "Orb":
                        return this.GhostOrbIcon48;
                    case "Box":
                        return this.SpiritBoxIcon48;
                    case "Dots":
                        return this.DotsIcon48;
                }
            } else {
                switch(name) {
                    case "EMF":
                        return this.EMFReaderIcon48g;
                    case "Finger":
                        return this.FingerprintIcon48g;
                    case "Book":
                        return this.BookIcon48g;
                    case "Temp":
                        return this.BreathingIcon48g;
                    case "Orb":
                        return this.GhostOrbIcon48g;
                    case "Box":
                        return this.SpiritBoxIcon48g;
                    case "Dots":
                        return this.DotsIcon48g;
                }
            }
            return (Bitmap)null;
        }

        private void GetGhostNames() {
            foreach(Helper_Ghosts ghost in this.GhostList)
                this.cbGhosts.Items.Add((object)ghost.Ghostname);
        }
        private void GetInfo() {
            foreach(Helper_Tables table in this.TableList) {
                this.cbInfo.Items.Add((object)table.Title);
            }
        }
        private void GetMaps() {
            if(this.var_dif==1) {
                foreach(Helper_Timer timer in this.TimerList)
                    this.cbMaps.Items.Add((object)timer.Dif_Amateur);
            } else if(this.var_dif==2) {
                foreach(Helper_Timer timer in this.TimerList)
                    this.cbMaps.Items.Add((object)timer.Dif_Intermediate);
            } else if(this.var_dif==3) {
                foreach(Helper_Timer timer in this.TimerList)
                    this.cbMaps.Items.Add((object)timer.Dif_Professional);
            } else if(this.var_dif==4) {
                foreach(Helper_Timer timer in this.TimerList)
                    this.cbMaps.Items.Add((object)timer.Dif_Nightmare);
            }

            foreach(Helper_Timer timer in this.TimerList) {
                string[] strArray1=timer.Small.Split(',');
                string[] strArray2=timer.Medium.Split(',');
                string[] strArray3=timer.Big.Split(',');

                foreach(string str1 in strArray1) {
                    string str2=str1;
                    if(str2.Substring(0,1)==" ")
                        str2=str1.Remove(0,1);
                    this.cbMaps.Items.Add((object)str2);
                }
                foreach(string str1 in strArray2) {
                    string str2=str1;
                    if(str2.Substring(0,1)==" ")
                        str2=str1.Remove(0,1);
                    this.cbMaps.Items.Add((object)str2);
                }
                foreach(string str1 in strArray3) {
                    string str2=str1;
                    if(str2.Substring(0,1)==" ")
                        str2=str1.Remove(0,1);
                    this.cbMaps.Items.Add((object)str2);
                }
            }
        }
        private void GetTimerTypes() {
            this.cbTimerType.Items.Add((object)" ");
            this.cbTimerType.Items.Add((object)"⏱");
            this.cbTimerType.Items.Add((object)this.TimerList[0].Timer_Hunt);
            this.cbTimerType.Items.Add((object)this.TimerList[0].Timer_Hunt_Cursed);
            this.cbTimerType.Items.Add((object)this.TimerList[0].Timer_Hunt_Post);
            this.cbTimerType.Items.Add((object)this.TimerList[0].Timer_Hunt_Post_Demon);
            this.cbTimerType.Items.Add((object)this.TimerList[0].Timer_Hunt_Smudge);
            this.cbTimerType.Items.Add((object)this.TimerList[0].Timer_Smudge_Demon);
            this.cbTimerType.Items.Add((object)this.TimerList[0].Timer_Smudge_Spirit);
            this.cbTimerType.Items.Add((object)this.TimerList[0].Timer_Start);
        }
        private void GetObjectives() {
            try {
                this.cbTask1.Items.AddRange((object[])File.ReadAllLines("Data/"+this.var_language+"/Objectives.txt"));
                this.cbTask2.Items.AddRange((object[])File.ReadAllLines("Data/"+this.var_language+"/Objectives.txt"));
                this.cbTask3.Items.AddRange((object[])File.ReadAllLines("Data/"+this.var_language+"/Objectives.txt"));
            } catch {}
        }
        private void GetAnswers() {
            try {
                this.cbAnswerType.Items.AddRange((object[])File.ReadAllLines("Data/"+this.var_language+"/Answer.txt"));
            } catch {}
        }

        private void ActivateMain() {
            this.btnEMF.Show();
            this.btnFinger.Show();
            this.btnBook.Show();
            this.btnTemp.Show();
            this.btnOrb.Show();
            this.btnBox.Show();
            this.btnDots.Show();
            
            this.btnMin.Show();
            this.btnExit.Show();
            this.btnHelp.Show();
            this.btnReset.Show();
            this.btnSettings.Show();
            this.btnGhostInfo.Show();

            this.cbFirstName.Show();
            this.cbLastName.Show();
            this.cbAnswerType.Show();
            this.cbTask1.Show();
            this.cbTask2.Show();
            this.cbTask3.Show();
            this.cbGhosts.Show();
            this.cbTimerType.Show();
            this.cbMaps.Show();
            
            this.lblDif.Show();
            this.lblHint.Show();
            this.lblGhInfoType.Show();
            this.lblGhInfoTypeData.Show();
            this.lblGhInfoHunt.Show();
            this.lblGhInfoHuntData.Show();
            this.lblGhInfoHuntAbilities.Show();
            this.lblGhInfoHuntAbilitiesData.Show();
            this.lblGhInfoCooldown.Show();
            this.lblGhInfoCooldownData.Show();

            this.picLine.Show();
            this.picGhostEvi1.Show();
            this.picGhostEvi2.Show();
            this.picGhostEvi3.Show();
            this.picGhostEvi4.Show();

            this.rtbGhostTyp.Show();
            this.rtbGhInfoHintsData.Show();
            
            this.tbCounter.Show();
        }
        private void DeactivateMain() {
            this.btnEMF.Hide();
            this.btnFinger.Hide();
            this.btnBook.Hide();
            this.btnTemp.Hide();
            this.btnOrb.Hide();
            this.btnBox.Hide();
            this.btnDots.Hide();

            this.btnMin.Hide();
            this.btnExit.Hide();
            this.btnHelp.Hide();
            this.btnReset.Hide();
            this.btnSettings.Hide();
            this.btnGhostInfo.Hide();

            this.cbFirstName.Hide();
            this.cbLastName.Hide();
            this.cbAnswerType.Hide();
            this.cbTask1.Hide();
            this.cbTask2.Hide();
            this.cbTask3.Hide();
            this.cbGhosts.Hide();
            this.cbTimerType.Hide();
            this.cbMaps.Hide();
            
            this.lblDif.Hide();
            this.lblHint.Hide();
            this.lblGhInfoType.Hide();
            this.lblGhInfoTypeData.Hide();
            this.lblGhInfoHunt.Hide();
            this.lblGhInfoHuntData.Hide();
            this.lblGhInfoHuntAbilities.Hide();
            this.lblGhInfoHuntAbilitiesData.Hide();
            this.lblGhInfoCooldown.Hide();
            this.lblGhInfoCooldownData.Hide();

            this.picLine.Hide();
            this.picGhostEvi1.Hide();
            this.picGhostEvi2.Hide();
            this.picGhostEvi3.Hide();
            this.picGhostEvi4.Hide();

            this.rtbGhostTyp.Hide();
            this.rtbGhInfoHintsData.Hide();

            this.tbCounter.Hide();
        }

        //Actions
        //Buttons
        private void btnEMF_MouseUp(object sender,MouseEventArgs e) {
            if(e.Button==MouseButtons.Left) {
                if(this.EviListHold.Contains("EMF"))
                    this.EviListHold.Remove("EMF");
                else if(!this.EviListGreen.Contains("EMF") && !this.EviListRed.Contains("EMF"))
                    this.EviListGreen.Add("EMF");
                else if(this.EviListGreen.Contains("EMF"))
                    this.EviListGreen.Remove("EMF");
            } else if(e.Button==MouseButtons.Right) {
                if(!this.EviListHold.Contains("EMF")) {
                    this.EviListHold.Add("EMF");
                    if(this.EviListGreen.Contains("EMF"))
                        this.EviListGreen.Remove("EMF");
                } else if(this.EviListHold.Contains("EMF"))
                    this.EviListHold.Remove("EMF");
            }
            this.check_ghosttyp();
        }
        private void btnFinger_MouseUp(object sender,MouseEventArgs e) {
            if(e.Button==MouseButtons.Left) {
                if(this.EviListHold.Contains("Finger"))
                    this.EviListHold.Remove("Finger");
                else if(!this.EviListGreen.Contains("Finger") && !this.EviListRed.Contains("Finger"))
                    this.EviListGreen.Add("Finger");
                else if(this.EviListGreen.Contains("Finger"))
                    this.EviListGreen.Remove("Finger");
            } else if(e.Button==MouseButtons.Right) {
                if(!this.EviListHold.Contains("Finger")) {
                    this.EviListHold.Add("Finger");
                    if(this.EviListGreen.Contains("Finger"))
                        this.EviListGreen.Remove("Finger");
                } else if(this.EviListHold.Contains("Finger"))
                    this.EviListHold.Remove("Finger");
            }
            this.check_ghosttyp();
        }
        private void btnBook_MouseUp(object sender,MouseEventArgs e) {
            if(e.Button==MouseButtons.Left) {
                if(this.EviListHold.Contains("Book"))
                    this.EviListHold.Remove("Book");
                else if(!this.EviListGreen.Contains("Book") && !this.EviListRed.Contains("Book"))
                    this.EviListGreen.Add("Book");
                else if(this.EviListGreen.Contains("Book"))
                    this.EviListGreen.Remove("Book");
            } else if(e.Button==MouseButtons.Right) {
                if(!this.EviListHold.Contains("Book")) {
                    this.EviListHold.Add("Book");
                    if(this.EviListGreen.Contains("Book"))
                        this.EviListGreen.Remove("Book");
                } else if(this.EviListHold.Contains("Book"))
                    this.EviListHold.Remove("Book");
            }
            this.check_ghosttyp();
        }
        private void btnTemp_MouseUp(object sender,MouseEventArgs e) {
            if(e.Button==MouseButtons.Left) {
                if(this.EviListHold.Contains("Temp"))
                    this.EviListHold.Remove("Temp");
                else if(!this.EviListGreen.Contains("Temp") && !this.EviListRed.Contains("Temp"))
                    this.EviListGreen.Add("Temp");
                else if(this.EviListGreen.Contains("Temp"))
                    this.EviListGreen.Remove("Temp");
            } else if(e.Button==MouseButtons.Right) {
                if(!this.EviListHold.Contains("Temp")) {
                    this.EviListHold.Add("Temp");
                    if(this.EviListGreen.Contains("Temp"))
                        this.EviListGreen.Remove("Temp");
                } else if(this.EviListHold.Contains("Temp"))
                    this.EviListHold.Remove("Temp");
            }
            this.check_ghosttyp();
        }
        private void btnOrb_MouseUp(object sender,MouseEventArgs e) {
            if(e.Button==MouseButtons.Left) {
                if(this.EviListHold.Contains("Orb"))
                    this.EviListHold.Remove("Orb");
                else if(!this.EviListGreen.Contains("Orb") && !this.EviListRed.Contains("Orb"))
                    this.EviListGreen.Add("Orb");
                else if(this.EviListGreen.Contains("Orb"))
                    this.EviListGreen.Remove("Orb");
            } else if(e.Button==MouseButtons.Right) {
                if(!this.EviListHold.Contains("Orb")) {
                    this.EviListHold.Add("Orb");
                    if(this.EviListGreen.Contains("Orb"))
                        this.EviListGreen.Remove("Orb");
                } else if(this.EviListHold.Contains("Orb"))
                    this.EviListHold.Remove("Orb");
            }
            this.check_ghosttyp();
        }
        private void btnBox_MouseUp(object sender,MouseEventArgs e) {
            if(e.Button==MouseButtons.Left) {
                if(this.EviListHold.Contains("Box"))
                    this.EviListHold.Remove("Box");
                else if(!this.EviListGreen.Contains("Box") && !this.EviListRed.Contains("Box"))
                    this.EviListGreen.Add("Box");
                else if(this.EviListGreen.Contains("Box"))
                    this.EviListGreen.Remove("Box");
            } else if(e.Button==MouseButtons.Right) {
                if(!this.EviListHold.Contains("Box")) {
                    this.EviListHold.Add("Box");
                    if(this.EviListGreen.Contains("Box"))
                        this.EviListGreen.Remove("Box");
                } else if(this.EviListHold.Contains("Box"))
                    this.EviListHold.Remove("Box");
            }
            this.check_ghosttyp();
        }
        private void btnDots_MouseUp(object sender,MouseEventArgs e) {
            if(e.Button==MouseButtons.Left) {
                if(this.EviListHold.Contains("Dots"))
                    this.EviListHold.Remove("Dots");
                else if(!this.EviListGreen.Contains("Dots") && !this.EviListRed.Contains("Dots"))
                    this.EviListGreen.Add("Dots");
                else if(this.EviListGreen.Contains("Dots"))
                    this.EviListGreen.Remove("Dots");
            } else if(e.Button==MouseButtons.Right) {
                if(!this.EviListHold.Contains("Dots")) {
                    this.EviListHold.Add("Dots");
                    if(this.EviListGreen.Contains("Dots"))
                        this.EviListGreen.Remove("Dots");
                } else if(this.EviListHold.Contains("Dots"))
                    this.EviListHold.Remove("Dots");
            }
            this.check_ghosttyp();
        }

        private void btnMin_Click(object sender,EventArgs e) {
            this.WindowState=FormWindowState.Minimized;    
        }
        private void btnExit_Click(object sender,EventArgs e) {
            this.hkRecorder.CleanHotKeys();
            Process.GetCurrentProcess().Kill();
        }
        private void btnHintClose_Click(object sender,EventArgs e) {
            this.cbInfo.Hide();
            this.rtbInfo.Hide();
            this.btnHintClose.Hide();
            this.ActivateMain();
            this.set_size_and_scale();
            this.RoundedCorners();

            if(this.var_last_toggleInfo==1 && (int)Settings.Default["CompactMode"]==0) {
                this.btnGhostInfo.BackgroundImage=(Image)this.arrow_up;
                this.var_toggleInfo=1;
                this.show_info();
                this.RoundedCorners();
            } else if(this.var_last_toggleInfo==0 ||(int)Settings.Default["CompactMode"]==0) {
                this.btnGhostInfo.BackgroundImage=(Image)this.arrow_down;
                this.var_toggleInfo=0;
                this.show_info();
                this.RoundedCorners();
            }
        }
        private void btnSetClose_Click(object sender,EventArgs e)   {
            this.panel_settings.Hide();
            this.ActivateMain();
            this.set_size_and_scale();
            this.RoundedCorners();

            if(this.var_last_toggleInfo==1 && (int)Settings.Default["CompactMode"]==0) {
                this.btnGhostInfo.BackgroundImage=(Image)this.arrow_up;
                this.var_toggleInfo=1;
                this.show_info();
                this.RoundedCorners();
            }else if(this.var_last_toggleInfo==0 && (int)Settings.Default["CompactMode"]==0) {
                this.btnGhostInfo.BackgroundImage=(Image)this.arrow_down;
                this.var_toggleInfo=0;
                this.show_info();
                this.RoundedCorners();
            }
        }
        private void btnOK_Click(object sender,EventArgs e) {
            Settings.Default["KeyID"]=(object)this.tbKeyID.Text;
            Settings.Default["TimerID"]=(object)this.tbTimerID.Text;
            Settings.Default["Lang"]=(object)this.cbLanguage.SelectedIndex;
            Settings.Default["Opacity"]=(object)this.Opacity_Value;
            Settings.Default["Size"]=(object)this.Size_Value;

            Settings.Default["TextColorR"]=(object)Convert.ToInt32(this.tbTextColorR.Text);
            Settings.Default["TextColorG"]=(object)Convert.ToInt32(this.tbTextColorG.Text);
            Settings.Default["TextColorB"]=(object)Convert.ToInt32(this.tbTextColorB.Text);

            Settings.Default["BoxForeColorR"]=(object)Convert.ToInt32(this.tbBoxForeColorR.Text);
            Settings.Default["BoxForeColorG"]=(object)Convert.ToInt32(this.tbBoxForeColorG.Text);
            Settings.Default["BoxForeColorB"]=(object)Convert.ToInt32(this.tbBoxForeColorB.Text);

            Settings.Default["BoxBackColorR"]=(object)Convert.ToInt32(this.tbBoxBackColorR.Text);
            Settings.Default["BoxBackColorG"]=(object)Convert.ToInt32(this.tbBoxBackColorG.Text);
            Settings.Default["BoxBackColorB"]=(object)Convert.ToInt32(this.tbBoxBackColorB.Text);

            Settings.Default["HintTextColorR"]=(object)Convert.ToInt32(this.tbHintTextColorR.Text);
            Settings.Default["HintTextColorG"]=(object)Convert.ToInt32(this.tbHintTextColorG.Text);
            Settings.Default["HintTextColorB"]=(object)Convert.ToInt32(this.tbHintTextColorB.Text);

            Settings.Default["BackgroundColorR"]=(object)Convert.ToInt32(this.tbBackgroundColorR.Text);
            Settings.Default["BackgroundColorG"]=(object)Convert.ToInt32(this.tbBackgroundColorG.Text);
            Settings.Default["BackgroundColorB"]=(object)Convert.ToInt32(this.tbBackgroundColorB.Text);

            Settings.Default["MousePos"]=(object)Cursor.Position;
            if(this.chbSoundOnOff.Checked)
                Settings.Default["SoundOnOff"]=(object)1;
            else
                Settings.Default["SoundOnOff"]=(object)0;
            if(this.chbCompact.Checked)
                Settings.Default["CompactMode"]=(object)1;
            else
                Settings.Default["CompactMode"]=(object)0;
            Settings.Default.Save();

            this.panel_settings.Hide();
            this.Opacity=(double)Settings.Default["Opacity"]/100.0;
            this.UserColors();
            this.Visible=true;
            this.var_keep_seconds=1;
            this.setting_language();
            this.load_files();

            this.check_ghosttyp();

            this.cbTask1.Items.Clear();
            this.cbTask2.Items.Clear();
            this.cbTask3.Items.Clear();
            this.GetObjectives();

            this.cbMaps.Items.Clear();
            this.GetMaps();

            this.cbTimerType.Items.Clear();
            this.GetTimerTypes();
            
            this.cbGhosts.Items.Clear();
            this.GetGhostNames();

            this.cbAnswerType.Items.Clear();
            this.GetAnswers();

            this.cbInfo.Items.Clear();
            this.GetInfo();
            
            this.cbGhosts_SelectedIndexGetIndex();

            this.WindowState=FormWindowState.Normal;
            Cursor.Position=(Point)Settings.Default["MousePos"];
            this.lblFocus.Focus();
            this.hkRecorder.CleanHotKeys();
            TypeConverter converter=TypeDescriptor.GetConverter(typeof(Keys));
            HotKey hotKey1=this.hkFactory.CreateHotKey((int)converter.ConvertFromString(Settings.Default["KeyID"].ToString())).GetHotKey();
            HotKey hotKey2=this.hkFactory.CreateHotKey((int)converter.ConvertFromString(Settings.Default["TimerID"].ToString())).GetHotKey();
            this.hkRecorder.RegisterHotKey(hotKey1);
            this.hkRecorder.RegisterHotKey(hotKey2);
            if(!this.hkListener.IsListening)
                this.hkListener.StartListening();
            this.ActivateMain();
            this.set_size_and_scale();
            this.RoundedCorners();
            if(this.var_last_toggleInfo==1 &&(int)Settings.Default["CompactMode"]==0) {
                this.btnGhostInfo.BackgroundImage=(Image)this.arrow_up;
                this.var_toggleInfo=1;
                this.show_info();
                this.RoundedCorners();
            }else if(this.var_last_toggleInfo==0 &&(int)Settings.Default["CompactMode"]==0) {
                this.btnGhostInfo.BackgroundImage=(Image)this.arrow_down;
                this.var_toggleInfo=0;
                this.show_info();
                this.RoundedCorners();
            }
            this.var_keep_seconds=0;
        }
        private void btnHelp_Click(object sender,EventArgs e) {
            this.var_last_size=(int)Settings.Default["Size"];
            this.BackgroundImage=(Image)null;
            this.UserColors();
            this.DeactivateMain();
            this.btnHintClose.Show();
            this.set_font_size_help_panel();
            this.cbInfo.Show();
            this.rtbInfo.Show();
            this.var_last_toggleInfo=this.var_toggleInfo;
            if(this.var_toggleInfo==0) {
                this.var_toggleInfo=1;
                this.show_info();
                this.RoundedCorners();
            }
        }
        private void btnReset_Click(object sender,EventArgs e) {
            this.cbFirstName.SelectedIndex=0;
            this.cbLastName.SelectedIndex=0;
            this.cbAnswerType.SelectedIndex=0;
            this.cbTask1.SelectedIndex=0;
            this.cbTask2.SelectedIndex=0;
            this.cbTask3.SelectedIndex=0;
            this.EviListGreen.Clear();
            this.EviListRed.Clear();
            this.EviListHold.Clear();
            this.var_countGhosts=0;
            this.lblHint.Visible=false;
            this.check_ghosttyp();
        }
        private void btnResetColor_Click(object sender,EventArgs e) {
            this.tbTextColorR.Text="255";
            this.tbTextColorG.Text="255";
            this.tbTextColorB.Text="255";

            this.tbBoxForeColorR.Text="255";
            this.tbBoxForeColorG.Text="255";
            this.tbBoxForeColorB.Text="255";

            this.tbBoxBackColorR.Text="1";
            this.tbBoxBackColorG.Text="5";
            this.tbBoxBackColorB.Text="9";

            this.tbHintTextColorR.Text="96";
            this.tbHintTextColorG.Text="214";
            this.tbHintTextColorB.Text="138";

            this.tbBackgroundColorR.Text="1";
            this.tbBackgroundColorG.Text="5";
            this.tbBackgroundColorB.Text="9";

            this.set_rgb_boxes();
        }
        private void btnSettings_Click(object sender,EventArgs e) {
            this.chbSoundOnOff.Checked=(int)Settings.Default["SoundOnOff"]==1;
            this.chbCompact.Checked=(int)Settings.Default["CompactMode"]==1;
            this.var_last_size=(int)Settings.Default["Size"];
            this.var_last_toggleInfo=this.var_toggleInfo;

            if(this.var_toggleInfo==1) {
                this.btnGhostInfo.BackgroundImage=(Image)this.arrow_down;
                this.var_toggleInfo=0;
                this.show_info();
                this.RoundedCorners();
            }
            this.DeactivateMain();
            this.set_size_and_scale();
            this.panel_settings.Show();
            this.panel_settings_load();
        }
        private void btnGhostInfo_Click(object sender,EventArgs e) {
            if(this.var_toggleInfo==0) {
                this.btnGhostInfo.BackgroundImage=(Image)this.arrow_up;
                this.var_toggleInfo=1;
                this.show_info();
                this.RoundedCorners();
            } else if(this.var_toggleInfo==1) {
                this.btnGhostInfo.BackgroundImage=(Image)this.arrow_down;
                this.var_toggleInfo=0;
                this.show_info();
                this.RoundedCorners();
            }
        }

        //Color
        private void Color_rgb_MouseDown(object sender,MouseEventArgs e) {
            this.lblFocus.Focus();
            this.btnHintClose.Focus();
            if(e.Button==MouseButtons.Left) {
                if(this.colorDialog.ShowDialog()==DialogResult.OK) {
                    byte num;
                    if(sender.Equals((object)this.tbTextColor_rgb)) {
                        TextBox tbTextColorR=this.tbTextColorR;
                        num=this.colorDialog.Color.R;
                        string str=num.ToString();
                        tbTextColorR.Text=str;
                        
                        TextBox tbTextColorG=this.tbTextColorG;
                        num=this.colorDialog.Color.G;
                        str=num.ToString();
                        tbTextColorG.Text=str;

                        TextBox tbTextColorB=this.tbTextColorB;
                        num=this.colorDialog.Color.B;
                        str=num.ToString();
                        tbTextColorB.Text=str;
                    }
                    if(sender.Equals((object)this.tbBoxForeColor_rgb)) {
                        TextBox tbBoxForeColorR=this.tbBoxForeColorR;
                        num=this.colorDialog.Color.R;
                        string str=num.ToString();
                        tbBoxForeColorR.Text=str;
                        
                        TextBox tbBoxForeColorG=this.tbBoxForeColorG;
                        num=this.colorDialog.Color.G;
                        str=num.ToString();
                        tbBoxForeColorG.Text=str;

                        TextBox tbBoxForeColorB=this.tbBoxForeColorB;
                        num=this.colorDialog.Color.B;
                        str=num.ToString();
                        tbBoxForeColorB.Text=str;
                    }
                    if(sender.Equals((object)this.tbBoxBackColor_rgb)) {
                        TextBox tbBoxBackColorR=this.tbBoxBackColorR;
                        num=this.colorDialog.Color.R;
                        string str=num.ToString();
                        tbBoxBackColorR.Text=str;
                        
                        TextBox tbBoxBackColorG=this.tbBoxBackColorG;
                        num=this.colorDialog.Color.G;
                        str=num.ToString();
                        tbBoxBackColorG.Text=str;

                        TextBox tbBoxBackColorB=this.tbBoxBackColorB;
                        num=this.colorDialog.Color.B;
                        str=num.ToString();
                        tbBoxBackColorB.Text=str;
                    }
                    if(sender.Equals((object)this.tbHintTextColor_rgb)) {
                        TextBox tbHintTextColorR=this.tbHintTextColorR;
                        num=this.colorDialog.Color.R;
                        string str=num.ToString();
                        tbHintTextColorR.Text=str;
                        
                        TextBox tbHintTextColorG=this.tbHintTextColorG;
                        num=this.colorDialog.Color.G;
                        str=num.ToString();
                        tbHintTextColorG.Text=str;

                        TextBox tbHintTextColorB=this.tbHintTextColorB;
                        num=this.colorDialog.Color.B;
                        str=num.ToString();
                        tbHintTextColorB.Text=str;
                    }
                    if(sender.Equals((object)this.tbBackgroundColor_rgb)) {
                        TextBox tblBackgroundColorR=this.tbBackgroundColorR;
                        num=this.colorDialog.Color.R;
                        string str=num.ToString();
                        tblBackgroundColorR.Text=str;
                        
                        TextBox tblBackgroundColorG=this.tbBackgroundColorG;
                        num=this.colorDialog.Color.G;
                        str=num.ToString();
                        tblBackgroundColorG.Text=str;

                        TextBox tblBackgroundColorB=this.tbBackgroundColorB;
                        num=this.colorDialog.Color.B;
                        str=num.ToString();
                        tblBackgroundColorB.Text=str;
                    }
                }
            }
            this.set_rgb_boxes();    
        }

        //Combo Boxes
        private void cbFirstName_SelectedIndexChanged(object sender,EventArgs e) {
            this.lblFocus.Focus();
        }

        private void cbLastName_SelectedIndexChanged(object sender,EventArgs e) {
            this.lblFocus.Focus();
        }

        private void cbAnswerType_SelectedIndexChanged(object sender,EventArgs e) {
            this.cbAnswerType_SelectedIndexGetIndex(cbAnswerType);
            this.lblFocus.Focus();
        }
        private void cbAnswerType_SelectedIndexGetIndex(ComboBox cb) {
            this.var_selectedIndexAnswers=cb.SelectedIndex;
        }

        private void cbTask1_SelectedIndexChanged(object sender,EventArgs e) {
            this.cbTasks_SelectedIndexGetIndex(this.cbTask1,1);
            this.lblFocus.Focus();
        }
        private void cbTask2_SelectedIndexChanged(object sender,EventArgs e) {
            this.cbTasks_SelectedIndexGetIndex(this.cbTask2,2);
            this.lblFocus.Focus();
        }
        private void cbTask3_SelectedIndexChanged(object sender,EventArgs e) {
            this.cbTasks_SelectedIndexGetIndex(this.cbTask3,3);
            this.lblFocus.Focus();
        }
        private void cbTasks_SelectedIndexGetIndex(ComboBox cb,int index) {
            if(index==1)
                this.var_selectedIndex1=cb.SelectedIndex;
            if(index==2)
                this.var_selectedIndex2=cb.SelectedIndex;
            if(index!=3)
                return;
            this.var_selectedIndex3=cb.SelectedIndex;
        }

        private void cbGhosts_SelectedIndexChanged(object sender,EventArgs e) {
            foreach(Helper_Ghosts ghost in this.GhostList) {
                if(this.cbGhosts.SelectedItem.ToString()==ghost.Ghostname) {
                    this.lblGhInfoType.Text=ghost.Type_Title;
                    this.lblGhInfoTypeData.Text=ghost.Type;
                    this.lblGhInfoHunt.Text=ghost.Hunt_Title;
                    this.lblGhInfoHuntData.Text=ghost.Hunt;
                    this.lblGhInfoHuntAbilities.Text=ghost.Hunt_Abilities_Title;
                    this.lblGhInfoHuntAbilitiesData.Text=ghost.Hunt_Abilities;
                    this.lblGhInfoCooldown.Text=ghost.Smudging_Title;
                    this.lblGhInfoCooldownData.Text=ghost.Smudging;
                    this.rtbGhInfoHintsData.Text=String.Join("\n",ghost.Hints);

                    this.picGhostEvi1.BackgroundImage=(Image)this.getPic(ghost.Evidence1,0);
                    this.picGhostEvi1.Tag=(object)ghost.Evidence1;
                    this.picGhostEvi2.BackgroundImage=(Image)this.getPic(ghost.Evidence2,0);
                    this.picGhostEvi2.Tag=(object)ghost.Evidence2;
                    this.picGhostEvi3.BackgroundImage=(Image)this.getPic(ghost.Evidence3,0);
                    this.picGhostEvi3.Tag=(object)ghost.Evidence3;
                    this.picGhostEvi4.BackgroundImage=(Image)this.getPic(ghost.Evidence4,0);
                    this.picGhostEvi4.Tag=(object)ghost.Evidence4;

                    if(ghost.Evidence_Guaranteed.Length>2) {
                        if(this.picGhostEvi1.Tag.ToString()==ghost.Evidence_Guaranteed)
                            this.picGhostEvi1.BackgroundImage=(Image)this.getPic(ghost.Evidence1,1);
                        if(this.picGhostEvi2.Tag.ToString()==ghost.Evidence_Guaranteed)
                            this.picGhostEvi2.BackgroundImage=(Image)this.getPic(ghost.Evidence2,1);
                        if(this.picGhostEvi3.Tag.ToString()==ghost.Evidence_Guaranteed)
                            this.picGhostEvi3.BackgroundImage=(Image)this.getPic(ghost.Evidence3,1);
                        if(this.picGhostEvi4.Tag.ToString()==ghost.Evidence_Guaranteed)
                            this.picGhostEvi4.BackgroundImage=(Image)this.getPic(ghost.Evidence4,1);
                    }
                }
            }
            this.var_selectedIndex_Ghosts=this.cbGhosts.SelectedIndex;
            this.lblFocus.Focus();
        }
        private void cbGhosts_SelectedIndexGetIndex() {
            try {
                this.cbAnswerType.SelectedIndex=this.var_selectedIndexAnswers;
                this.cbTask1.SelectedIndex=this.var_selectedIndex1;
                this.cbTask2.SelectedIndex=this.var_selectedIndex2;
                this.cbTask3.SelectedIndex=this.var_selectedIndex3;
                this.cbGhosts.SelectedIndex=this.var_selectedIndex_Ghosts;
                this.cbInfo.SelectedIndex=this.var_selectedIndex_Info;
                this.cbMaps.SelectedIndex=this.var_selectedIndexMaps;

                int num=this.cbTimerType.FindString(this.var_selectedIndexTimerType);
                if(num!=-1)
                    this.cbTimerType.SelectedIndex=num;
                else
                    this.cbTimerType.SelectedIndex=1;
            } catch {}
        }

        private void cbInfo_SelectedIndexChanged(object sender,EventArgs e) {
            foreach(Helper_Tables table in this.TableList) {
                if(this.cbInfo.SelectedItem.ToString()==table.Title) {
                    this.rtbInfo.Text=String.Join("\n",table.Text);
                    this.var_selectedIndex_Info=this.cbInfo.SelectedIndex;
                }
            }
            this.btnHintClose.Focus();
        }

        private void cbTimerType_SelectedIndexChanged(object sender,EventArgs e) {
            this.cbTimerType_SelectedIndexGetIndex(this.cbTimerType);
            this.lblFocus.Focus();
        }
        private void cbTimerType_SelectedIndexGetIndex(ComboBox cbb) {
            if(this.var_keep_seconds!=0)
                return;
            string str1=" ";
            string str2=this.cbMaps.SelectedItem.ToString();
            string medium=this.TimerList[0].Medium;
            string big=this.TimerList[0].Big;
            int num=1;
            
            if(medium.Contains(str2))
                num=2;
            else if(big.Contains(this.cbMaps.SelectedItem.ToString()))
                num=3;

            if(this.custom_timer!=0 && this.cbTimerType.SelectedIndex==0) {
                if(this.timer.Enabled)
                    return;
                this.var_selectedIndexTimerType=" ";
                this.tbCounter.Text=this.custom_timer.ToString();
                this.var_seconds=int.Parse(this.tbCounter.Text)-1;
            } else {
                if(this.cbTimerType.SelectedIndex==1) {
                    if(this.tbCounter.Text.Contains(":"))
                        return;
                    this.var_selectedIndexTimerType="⏱";
                    this.tbCounter.Text="00:00";
                    this.var_seconds=0;
                }
                if(this.cbMaps.SelectedIndex!=0) {
                    if(cbb.SelectedItem.ToString()==" "){
                        this.var_selectedIndexTimerType=" ";
                        this.tbCounter.Text="0";
                        this.var_seconds=0;
                    }
                    if(cbb.SelectedItem.ToString()==this.TimerList[0].Timer_Hunt_Post){
                        this.var_selectedIndexTimerType=this.TimerList[0].Timer_Hunt_Post;
                        this.tbCounter.Text="25";
                        this.var_seconds=24;
                    }
                    if(cbb.SelectedItem.ToString()==this.TimerList[0].Timer_Hunt_Post_Demon){
                        this.var_selectedIndexTimerType=" ";
                        this.tbCounter.Text="20";
                        this.var_seconds=19;
                    }
                    if(cbb.SelectedItem.ToString()==this.TimerList[0].Timer_Hunt_Smudge){
                        this.var_selectedIndexTimerType=" ";
                        this.tbCounter.Text="90";
                        this.var_seconds=89;
                    }
                    if(cbb.SelectedItem.ToString()==this.TimerList[0].Timer_Smudge_Spirit){
                        this.var_selectedIndexTimerType=" ";
                        this.tbCounter.Text="180";
                        this.var_seconds=179;
                    }
                    if(cbb.SelectedItem.ToString()==this.TimerList[0].Timer_Smudge_Demon){
                        this.var_selectedIndexTimerType=" ";
                        this.tbCounter.Text="60";
                        this.var_seconds=59;
                    }
                }

                if(this.cbMaps.SelectedIndex==0 || cbb.SelectedItem.ToString()==str1 &&this.custom_timer==0) {
                    if(cbb.SelectedItem.ToString()==" ") {
                        this.var_selectedIndexTimerType=" ";
                        this.tbCounter.Text="🕒";
                        this.var_seconds=0;
                    }
                    if(cbb.SelectedItem.ToString()==this.TimerList[0].Timer_Start) {
                        this.var_selectedIndexTimerType=" ";
                        this.tbCounter.Text="🕒";
                        this.var_seconds=0;
                    }
                    if(cbb.SelectedItem.ToString()==this.TimerList[0].Timer_Hunt) {
                        this.var_selectedIndexTimerType=" ";
                        this.tbCounter.Text="🕒";
                        this.var_seconds=0;
                    }
                    if(cbb.SelectedItem.ToString()==this.TimerList[0].Timer_Hunt_Cursed) {
                        this.var_selectedIndexTimerType=" ";
                        this.tbCounter.Text="🕒";
                        this.var_seconds=0;
                    }
                    if(cbb.SelectedItem.ToString()==this.TimerList[0].Timer_Hunt_Post) {
                        this.var_selectedIndexTimerType=" ";
                        this.tbCounter.Text="🕒";
                        this.var_seconds=0;
                    }
                    if(cbb.SelectedItem.ToString()==this.TimerList[0].Timer_Hunt_Post_Demon) {
                        this.var_selectedIndexTimerType=" ";
                        this.tbCounter.Text="🕒";
                        this.var_seconds=0;
                    }
                    if(cbb.SelectedItem.ToString()==this.TimerList[0].Timer_Hunt_Smudge) {
                        this.var_selectedIndexTimerType=" ";
                        this.tbCounter.Text="🕒";
                        this.var_seconds=0;
                    }
                    if(cbb.SelectedItem.ToString()==this.TimerList[0].Timer_Smudge_Spirit) {
                        this.var_selectedIndexTimerType=" ";
                        this.tbCounter.Text="🕒";
                        this.var_seconds=0;
                    }
                    if(cbb.SelectedItem.ToString()==this.TimerList[0].Timer_Smudge_Demon) {
                        this.var_selectedIndexTimerType=" ";
                        this.tbCounter.Text="🕒";
                        this.var_seconds=0;
                    }
                }

                if(this.cbMaps.SelectedIndex!=0 &&cbb.SelectedItem.ToString()!=str1) {
                    if(this.var_dif==1) {
                        if(cbb.SelectedItem.ToString()==" ") {
                            this.var_selectedIndexTimerType=" ";
                            this.tbCounter.Text="0";
                            this.var_seconds=0;
                        }
                        if(cbb.SelectedItem.ToString()==this.TimerList[0].Timer_Start) {
                            this.var_selectedIndexTimerType=this.TimerList[0].Timer_Start;
                            this.tbCounter.Text="300";
                            this.var_seconds=299;
                        }
                        if(cbb.SelectedItem.ToString()==this.TimerList[0].Timer_Hunt) {
                            this.var_selectedIndexTimerType=this.TimerList[0].Timer_Hunt;
                            this.tbCounter.Text="15";
                            this.var_seconds=14;
                            if(num==2) {
                                this.tbCounter.Text="30";
                                this.var_seconds=29;
                            }
                            if(num==3) {
                                this.tbCounter.Text="40";
                                this.var_seconds=39;
                            }
                        }
                        if(cbb.SelectedItem.ToString()==this.TimerList[0].Timer_Hunt_Cursed) {
                            this.var_selectedIndexTimerType=this.TimerList[0].Timer_Hunt_Cursed;
                            this.tbCounter.Text="35";
                            this.var_seconds=34;
                            if(num==2) {
                                this.tbCounter.Text="50";
                                this.var_seconds=49;
                            }
                            if(num==3) {
                                this.tbCounter.Text="60";
                                this.var_seconds=59;
                            }
                        }
                    }
                    if(this.var_dif==2) {
                        if(cbb.SelectedItem.ToString()==" ") {
                            this.var_selectedIndexTimerType=" ";
                            this.tbCounter.Text="0";
                            this.var_seconds=0;
                        }
                        if(cbb.SelectedItem.ToString()==this.TimerList[0].Timer_Start) {
                            this.var_selectedIndexTimerType=this.TimerList[0].Timer_Start;
                            this.tbCounter.Text="120";
                            this.var_seconds=119;
                        }
                        if(cbb.SelectedItem.ToString()==this.TimerList[0].Timer_Hunt) {
                            this.var_selectedIndexTimerType=this.TimerList[0].Timer_Hunt;
                            this.tbCounter.Text="20";
                            this.var_seconds=19;
                            if(num==2) {
                                this.tbCounter.Text="40";
                                this.var_seconds=39;
                            }
                            if(num==3) {
                                this.tbCounter.Text="50";
                                this.var_seconds=49;
                            }
                        }
                        if(cbb.SelectedItem.ToString()==this.TimerList[0].Timer_Hunt_Cursed) {
                            this.var_selectedIndexTimerType=this.TimerList[0].Timer_Hunt_Cursed;
                            this.tbCounter.Text="40";
                            this.var_seconds=39;
                            if(num==2) {
                                this.tbCounter.Text="60";
                                this.var_seconds=59;
                            }
                            if(num==3) {
                                this.tbCounter.Text="70";
                                this.var_seconds=69;
                            }
                        }
                    }
                    if(this.var_dif==3 || this.var_dif==4) {
                        if(cbb.SelectedItem.ToString()==" ") {
                            this.var_selectedIndexTimerType=" ";
                            this.tbCounter.Text="0";
                            this.var_seconds=0;
                        }
                        if(cbb.SelectedItem.ToString()==this.TimerList[0].Timer_Start) {
                            this.var_selectedIndexTimerType=this.TimerList[0].Timer_Start;
                            this.tbCounter.Text="0";
                            this.var_seconds=0;
                        }
                        if(cbb.SelectedItem.ToString()==this.TimerList[0].Timer_Hunt) {
                            this.var_selectedIndexTimerType=this.TimerList[0].Timer_Hunt;
                            this.tbCounter.Text="30";
                            this.var_seconds=29;
                            if(num==2) {
                                this.tbCounter.Text="50";
                                this.var_seconds=49;
                            }
                            if(num==3) {
                                this.tbCounter.Text="60";
                                this.var_seconds=59;
                            }
                        }
                        if(cbb.SelectedItem.ToString()==this.TimerList[0].Timer_Hunt_Cursed) {
                            this.var_selectedIndexTimerType=this.TimerList[0].Timer_Hunt_Cursed;
                            this.tbCounter.Text="50";
                            this.var_seconds=49;
                            if(num==2) {
                                this.tbCounter.Text="70";
                                this.var_seconds=69;
                            }
                            if(num==3) {
                                this.tbCounter.Text="80";
                                this.var_seconds=79;
                            }
                        }
                    }
                }

                this.var_selectedIndexMaps=this.cbMaps.SelectedIndex;
                if(!this.timer.Enabled)
                    return;
                this.tbCounter.BackColor=Color.WhiteSmoke;
                this.timer.Stop();
                this.cbTimerType_SelectedIndexGetIndex(this.cbTimerType);
                this.lblFocus.Focus();
                this.beep_1.Stop();
                this.beep_2.Stop();
                this.beep_3.Stop();
                this.beep_finish.Stop();
            }
        }

        private void cbMaps_SelectedIndexChanged(object sender,EventArgs e) {
            this.cbMaps_SelectedIndexGetIndex();
            this.lblFocus.Focus();
        }
        private void cbMaps_SelectedIndexGetIndex() {
            if(!this.cbTimerType.CanSelect)
                return;
            this.cbTimerType_SelectedIndexGetIndex(this.cbTimerType);
        }

        private void cbLanguage_SelectedIndexChanged(object sender,EventArgs e) {
            this.btnSetClose.Focus();
        }

        //Labels
        private void lblDif_Click(object sender,MouseEventArgs e) {
            if(e.Button==MouseButtons.Left) {
                if(this.var_dif==1) {
                    this.lblDif.BackColor=ColorTranslator.FromHtml("#ffdc37");
                    this.var_dif=2;
                } else if(this.var_dif==2) {
                    this.lblDif.BackColor=ColorTranslator.FromHtml("#f19b55");
                    this.var_dif=3;
                } else if(this.var_dif==3) {
                    this.lblDif.BackColor=ColorTranslator.FromHtml("#d66060");
                    this.var_dif=4;
                } else if(this.var_dif==4) {
                    this.lblDif.BackColor=Color.WhiteSmoke;
                    this.var_dif=1;
                }
            } else if(e.Button==MouseButtons.Right) {
                if(this.var_dif==4) {
                    this.lblDif.BackColor=ColorTranslator.FromHtml("#f19b55");
                    this.var_dif=3;
                } else if(this.var_dif==3) {
                    this.lblDif.BackColor=ColorTranslator.FromHtml("#ffdc37");
                    this.var_dif=2;
                } else if(this.var_dif==2) {
                    this.lblDif.BackColor=Color.WhiteSmoke;
                    this.var_dif=1;
                } else if(this.var_dif==1) {
                    this.lblDif.BackColor=ColorTranslator.FromHtml("#d66060");
                    this.var_dif=4;
                }
            }

            int selectedIndex=this.cbMaps.SelectedIndex;
            this.cbMaps.Items.Clear();
            this.GetMaps();
            this.cbMaps.SelectedIndex=selectedIndex;
            this.lblDif_SelectedIndexGetIndex();
        }
        private void lblDif_Paint(object sender,PaintEventArgs e) {
            ControlPaint.DrawBorder(e.Graphics,this.lblDif.DisplayRectangle,ColorTranslator.FromHtml("#c0c0c0"),ButtonBorderStyle.Solid);
        }
        private void lblDif_SelectedIndexGetIndex() {
            if(!this.cbTimerType.CanSelect)
                return;
            this.cbTimerType_SelectedIndexGetIndex(this.cbTimerType);
        }

        //Main
        private void Main_MouseDown(object sender,MouseEventArgs e) {
            this.lblFocus.Focus();
            this.btnHintClose.Focus();
            if(e.Button==MouseButtons.Left){
                PhasmoGadget.ReleaseCapture();
                PhasmoGadget.SendMessage(this.Handle,161,2,0);

                if(Settings.Default["PositionX"]!=null && Settings.Default["PositionY"]!=null){
                    Point location=this.Location;
                    int x1=location.X;
                    int y1=location.Y;
                    if(x1!=(int)Settings.Default["PositionX"] && y1!=(int)Settings.Default["PositionY"]){
                        Settings settings1=Settings.Default;
                        location=this.Location;
                        ValueType x2=(ValueType)location.X;
                        settings1["PositionX"]=(object)x2;

                        Settings settings2=Settings.Default;
                        location=this.Location;
                        ValueType y2=(ValueType)location.Y;
                        settings2["PositionY"]=(object)y2;

                        Settings.Default.Save();    
                    }
                }
            }
        }

        //Panel
        private void panel_settings_MouseDown(object sender,MouseEventArgs e) {
            if(e.Button==MouseButtons.Left) {
                PhasmoGadget.ReleaseCapture();
                PhasmoGadget.SendMessage(this.Handle,161,2,0);
            }

            if(Settings.Default["PositionX"]==null || Settings.Default["PositionY"]==null)
                return;

            Point location=this.Location;
            int x1=location.X;
            location=this.Location;
            int y1=location.Y;
            int num=(int)Settings.Default["PositionX"];
            if(x1==num || y1==(int)Settings.Default["PositionY"])
                return;

            Settings settings1=Settings.Default;
            location=this.Location;
            ValueType x2=(ValueType)location.X;
            settings1["PositionX"]=(object)x2;
            Settings settings2=Settings.Default;
            location=this.Location;
            ValueType y2=(ValueType)location.X;
            settings2["PositionY"]=(object)y2;

            Settings.Default.Save();
        }

        //Text Boxes
        private void tbKeyID_KeyDown(object sender, KeyEventArgs e) {
            this.var_KeyID=Convert.ToString((object)e.KeyCode);
            this.tbKeyID.Text=this.var_KeyID;
        }

        private void tbTimerID_KeyDown(object sender, KeyEventArgs e) {
            this.var_TimerID=Convert.ToString((object)e.KeyCode);
            this.tbTimerID.Text=this.var_TimerID;
        }

        private void tbCounter_MouseUp(object sender,MouseEventArgs e) {
            if(e.Button==MouseButtons.Left) {
                this.lblFocus.Focus();
                this.tbCounter.ReadOnly=true;

                if(this.cbTimerType.SelectedIndex!=1) {
                    if(!this.timer.Enabled) {
                        if(this.var_seconds>0) {
                            this.tbCounter.BackColor=ColorTranslator.FromHtml("#60d68a");
                            this.timer.Enabled=true;
                            this.timer.Start();
                            if((int)Settings.Default["SoundOnOff"]==1)
                                this.beep_1.Play();
                            this.lblFocus.Focus();
                        }
                    } else if(this.timer.Enabled) {
                        this.tbCounter.BackColor=Color.WhiteSmoke;
                        this.timer.Stop();
                        this.cbTimerType_SelectedIndexGetIndex(this.cbTimerType);
                        if(this.custom_timer!=0 &&this.cbTimerType.SelectedIndex==0) {
                            this.tbCounter.Text=this.custom_timer.ToString();
                            this.var_seconds=int.Parse(this.tbCounter.Text)-1;
                        }
                    }
                } else if(this.cbTimerType.SelectedIndex==1) {
                    if(!this.timer.Enabled) {
                        this.tbCounter.Text="00:00";
                        this.var_seconds=0;
                        this.tbCounter.BackColor=ColorTranslator.FromHtml("#60d68a");
                        this.timer.Enabled=true;
                        this.timer.Start();
                        if((int)Settings.Default["SoundOnOff"]==1)
                            this.beep_1.Play();
                        this.lblFocus.Focus();
                    } else if(this.timer.Enabled) {
                        this.tbCounter.BackColor=Color.WhiteSmoke;
                        this.timer.Stop();
                        this.cbTimerType_SelectedIndexGetIndex(this.cbTimerType);
                        this.lblFocus.Focus();
                    }
                }
            }
            if(e.Button!=MouseButtons.Right)
                return;
            this.cbTimerType.SelectedIndex=0;
            this.tbCounter.BackColor=Color.WhiteSmoke;
            this.timer.Stop();
            this.cbTimerType_SelectedIndexGetIndex(this.cbTimerType);
            this.tbCounter.Clear();
            this.tbCounter.Focus();
            this.beep_1.Stop();
            this.beep_2.Stop();
            this.beep_3.Stop();
            this.beep_finish.Stop();
            this.tbCounter.ReadOnly=false;
        }
        private void tbCounter_KeyUp(object sender,KeyEventArgs e) {
            this.Input_Only9999(sender,e,this.tbCounter);
            if(this.tbCounter.Text==null)
                return;
            if(Regex.IsMatch(this.tbCounter.Text,"[0-9]")) {
                this.var_seconds=int.Parse(this.tbCounter.Text)-1;
                this.custom_timer=this.var_seconds+1;
            } else {
                this.tbCounter.Clear();
            }
        }
        private void tbCounter_KeyDown(object sender,KeyEventArgs e) {
            if(e.KeyCode!=Keys.Return)
                return;
            e.SuppressKeyPress=true;
        }
        private void tbCounter_KeyPress(object sender,KeyPressEventArgs e) {
            if(!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar!='.')
                e.Handled=true;
            if(e.KeyChar!='.' || (sender as TextBox).Text.IndexOf('.')<=-1)
                return;
            e.Handled=true;
        }

        private void tbTextColorR_KeyUp(object sender,KeyEventArgs e) {
            this.Input_Only255(sender,e,this.tbTextColorR);
            this.set_rgb_boxes();
        }
        private void tbTextColorG_KeyUp(object sender,KeyEventArgs e) {
            this.Input_Only255(sender,e,this.tbTextColorG);
            this.set_rgb_boxes();
        }
        private void tbTextColorB_KeyUp(object sender,KeyEventArgs e) {
            this.Input_Only255(sender,e,this.tbTextColorB);
            this.set_rgb_boxes();
        }

        private void tbBoxForeColorR_KeyUp(object sender,KeyEventArgs e) {
            this.Input_Only255(sender,e,this.tbBoxForeColorR);
            this.set_rgb_boxes();
        }
        private void tbBoxForeColorG_KeyUp(object sender,KeyEventArgs e) {
            this.Input_Only255(sender,e,this.tbBoxForeColorG);
            this.set_rgb_boxes();
        }
        private void tbBoxForeColorB_KeyUp(object sender,KeyEventArgs e) {
            this.Input_Only255(sender,e,this.tbBoxForeColorB);
            this.set_rgb_boxes();
        }

        private void tbBoxBackColorR_KeyUp(object sender,KeyEventArgs e) {
            this.Input_Only255(sender,e,this.tbBoxBackColorR);
            this.set_rgb_boxes();
        }
        private void tbBoxBackColorG_KeyUp(object sender,KeyEventArgs e) {
            this.Input_Only255(sender,e,this.tbBoxBackColorG);
            this.set_rgb_boxes();
        }
        private void tbBoxBackColorB_KeyUp(object sender,KeyEventArgs e) {
            this.Input_Only255(sender,e,this.tbBoxBackColorB);
            this.set_rgb_boxes();
        }

        private void tbHintTextColorR_KeyUp(object sender,KeyEventArgs e) {
            this.Input_Only255(sender,e,this.tbHintTextColorR);
            this.set_rgb_boxes();
        }
        private void tbHintTextColorG_KeyUp(object sender,KeyEventArgs e) {
            this.Input_Only255(sender,e,this.tbHintTextColorG);
            this.set_rgb_boxes();
        }
        private void tbHintTextColorB_KeyUp(object sender,KeyEventArgs e) {
            this.Input_Only255(sender,e,this.tbHintTextColorB);
            this.set_rgb_boxes();
        }

        private void tbBackgroundColorR_KeyUp(object sender,KeyEventArgs e) {
            this.Input_Only255(sender,e,this.tbBackgroundColorR);
            this.set_rgb_boxes();
        }
        private void tbBackgroundColorG_KeyUp(object sender,KeyEventArgs e) {
            this.Input_Only255(sender,e,this.tbBackgroundColorG);
            this.set_rgb_boxes();
        }
        private void tbBackgroundColorB_KeyUp(object sender,KeyEventArgs e) {
            this.Input_Only255(sender,e,this.tbBackgroundColorB);
            this.set_rgb_boxes();
        }

        //Timer
        private void timer_Tick(object sender,EventArgs e) {
            if(this.cbTimerType.SelectedIndex==1) {
                ++this.var_seconds;
                this.tbCounter.Text=TimeSpan.FromSeconds((double)this.var_seconds).ToString("mm\\:ss");
            } else {
                if(this.cbTimerType.SelectedIndex==1)
                    return;
                this.tbCounter.Text=this.var_seconds--.ToString();

                if(this.var_seconds<0) {
                    this.tbCounter.BackColor=Color.WhiteSmoke;
                    this.timer.Stop();
                    this.timer.Enabled=false;
                    this.cbTimerType_SelectedIndexGetIndex(this.cbTimerType);
                    this.lblFocus.Focus();
                    if((int)Settings.Default["SoundOnOff"]==1)
                        this.beep_finish.Play();
                    if(this.custom_timer==0 || this.cbTimerType.SelectedIndex!=0)
                        return;
                    this.tbCounter.Text=this.custom_timer.ToString();
                    this.var_seconds=int.Parse(this.tbCounter.Text)-1;
                } else if(this.var_seconds==4) {
                    if((int)Settings.Default["SoundOnOff"]==1)
                        this.beep_3.Play();
                } else if(this.var_seconds==9) {
                    if((int)Settings.Default["SoundOnOff"]==1)
                        this.beep_1.Play();
                } else {
                    if(this.var_seconds==19 && (int)Settings.Default["SoundOnOff"]==1)
                        this.beep_2.Play();
                }
            }
        }

        //Track Bars
        private void tbOpacity_Scroll(object sender,EventArgs e) {
            this.Opacity_Value=(double)(this.trbOpacity.Value*5);
            this.lblOpacity_Value.Text=this.Opacity_Value.ToString();
            this.Opacity=this.Opacity_Value/100.0;
        }

        private void tbSize_Scroll(object sender,EventArgs e) {
            this.Size_Value=this.trbSize.Value;
            this.lblSize_Value.Text=this.Size_Value.ToString();
        }

        //Other
        private void Input_Only255(object sender,KeyEventArgs e,TextBox tb) {
            if(tb.TextLength==0)
                tb.Text="0";
            if(tb.Text!=null && Convert.ToInt32(tb.Text)>(int)byte.MaxValue)
                tb.Text="255";
        }
        private void Input_Only9999(object sender,KeyEventArgs e,TextBox tb) {
            if(tb.TextLength==0)
                tb.Text="0";
            if(tb.Text==null || Convert.ToInt32(tb.Text)<=9999)
                return;
            tb.Text="9999";
        }
        private void Input_OnlyNum(object sender,KeyPressEventArgs e) {
            if(!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar!='.')
                e.Handled=true;
            if(e.KeyChar=='.' && (sender as TextBox).Text.IndexOf('.')>-1)
                e.Handled=true;
        }

        protected override void Dispose(bool disposing) {
            if(disposing && this.components!=null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            this.components=(IContainer)new System.ComponentModel.Container();
            ComponentResourceManager componentResourceManager=new ComponentResourceManager(typeof(PhasmoGadget));
            
            this.btnEMF=new Button();
            this.btnFinger=new Button();
            this.btnBook=new Button();
            this.btnTemp=new Button();
            this.btnOrb=new Button();
            this.btnBox=new Button();
            this.btnDots=new Button();
            this.btnMin=new Button();
            this.btnExit=new Button();
            this.btnHintClose=new Button();
            this.btnSetClose=new Button();
            this.btnOK=new Button();
            this.btnHelp=new Button();
            this.btnReset=new Button();
            this.btnResetColor=new Button();
            this.btnSettings=new Button();
            this.btnGhostInfo=new Button();

            this.chbCompact=new CheckBox();
            this.chbSoundOnOff=new CheckBox();

            this.colorDialog=new ColorDialog();

            this.cbFirstName=new ComboBox();
            this.cbLastName=new ComboBox();
            this.cbAnswerType=new ComboBox();
            this.cbTask1=new ComboBox();
            this.cbTask2=new ComboBox();
            this.cbTask3=new ComboBox();
            this.cbGhosts=new ComboBox();
            this.cbInfo=new ComboBox();
            this.cbTimerType=new ComboBox();
            this.cbMaps=new ComboBox();
            this.cbLanguage=new ComboBox();

            this.lblFocus=new Label();
            this.lblHint=new Label();
            this.lblDif=new Label();
            this.lblHotKey=new Label();
            this.lblTimerKey=new Label();
            this.lblLanguage=new Label();
            this.lblOpacity=new Label();
            this.lblOpacity_Value=new Label();
            this.lblSize=new Label();
            this.lblSize_Value=new Label();
            this.lblCopyright=new Label();
            this.lblVersion=new Label();
            this.lblTextColor=new Label();
            this.lblBoxForeColor=new Label();
            this.lblBoxBackColor=new Label();
            this.lblHintTextColor=new Label();
            this.lblBackgroundColor=new Label();
            this.lblGhInfoType=new Label();
            this.lblGhInfoTypeData=new Label();
            this.lblGhInfoHunt=new Label();
            this.lblGhInfoHuntData=new Label();
            this.lblGhInfoHuntAbilities=new Label();
            this.lblGhInfoHuntAbilitiesData=new Label();
            this.lblGhInfoCooldown=new Label();
            this.lblGhInfoCooldownData=new Label();

            this.panel_settings=new Panel();
            this.panel_settings.SuspendLayout();

            this.picLine=new PictureBox();
            this.picGhost=new PictureBox();
            this.picGhostEvi1=new PictureBox();
            this.picGhostEvi2=new PictureBox();
            this.picGhostEvi3=new PictureBox();
            this.picGhostEvi4=new PictureBox();

            this.rtbGhostTyp=new RichTextBox();
            this.rtbGhInfoHintsData=new RichTextBox();
            this.rtbInfo=new RichTextBox();

            this.timer=new Timer(this.components);
            
            this.tbKeyID=new TextBox();
            this.tbTimerID=new TextBox();
            this.tbCounter=new TextBox();
            this.tbTextColor_rgb=new TextBox();
            this.tbBoxForeColor_rgb=new TextBox();
            this.tbBoxBackColor_rgb=new TextBox();
            this.tbHintTextColor_rgb=new TextBox();
            this.tbBackgroundColor_rgb=new TextBox();
            this.tbTextColorR=new TextBox();
            this.tbTextColorG=new TextBox();
            this.tbTextColorB=new TextBox();
            this.tbBoxForeColorR=new TextBox();
            this.tbBoxForeColorG=new TextBox();
            this.tbBoxForeColorB=new TextBox();
            this.tbBoxBackColorR=new TextBox();
            this.tbBoxBackColorG=new TextBox();
            this.tbBoxBackColorB=new TextBox();
            this.tbHintTextColorR=new TextBox();
            this.tbHintTextColorG=new TextBox();
            this.tbHintTextColorB=new TextBox();
            this.tbBackgroundColorR=new TextBox();
            this.tbBackgroundColorG=new TextBox();
            this.tbBackgroundColorB=new TextBox();

            this.trbOpacity=new TrackBar();
            this.trbSize=new TrackBar();

            this.trbSize.BeginInit();
            this.trbOpacity.BeginInit();
            ((ISupportInitialize)this.picLine).BeginInit();
            ((ISupportInitialize)this.picGhost).BeginInit();
            ((ISupportInitialize)this.picGhostEvi1).BeginInit();
            ((ISupportInitialize)this.picGhostEvi2).BeginInit();
            ((ISupportInitialize)this.picGhostEvi3).BeginInit();
            ((ISupportInitialize)this.picGhostEvi4).BeginInit();
            this.SuspendLayout();

            //Buttons
            this.btnEMF.BackgroundImageLayout=ImageLayout.Stretch;
            this.btnEMF.Name="btnEMF";
            this.btnEMF.Size=new Size(48,48);
            this.btnEMF.TabIndex=1;
            this.btnEMF.UseVisualStyleBackColor=true;
            this.btnEMF.MouseUp+=new MouseEventHandler(this.btnEMF_MouseUp);
            
            this.btnFinger.BackgroundImageLayout=ImageLayout.Stretch;
            this.btnFinger.Name="btnFinger";
            this.btnFinger.Size=new Size(48,48);
            this.btnFinger.TabIndex=3;
            this.btnFinger.UseVisualStyleBackColor=true;
            this.btnFinger.MouseUp+=new MouseEventHandler(this.btnFinger_MouseUp);
            
            this.btnBook.BackgroundImageLayout=ImageLayout.Stretch;
            this.btnBook.Name="btnBook";
            this.btnBook.Size=new Size(48,48);
            this.btnBook.TabIndex=5;
            this.btnBook.UseVisualStyleBackColor=true;
            this.btnBook.MouseUp+=new MouseEventHandler(this.btnBook_MouseUp);

            this.btnTemp.BackgroundImageLayout=ImageLayout.Stretch;
            this.btnTemp.Name="btnTemp";
            this.btnTemp.Size=new Size(48,48);
            this.btnTemp.TabIndex=6;
            this.btnTemp.UseVisualStyleBackColor=true;
            this.btnTemp.MouseUp+=new MouseEventHandler(this.btnTemp_MouseUp);
            
            this.btnOrb.BackgroundImageLayout=ImageLayout.Stretch;
            this.btnOrb.Name="btnOrb";
            this.btnOrb.Size=new Size(48,48);
            this.btnOrb.TabIndex=4;
            this.btnOrb.UseVisualStyleBackColor=true;
            this.btnOrb.MouseUp+=new MouseEventHandler(this.btnOrb_MouseUp);

            this.btnBox.BackgroundImageLayout=ImageLayout.Stretch;
            this.btnBox.Name="btnBox";
            this.btnBox.Size=new Size(48,48);
            this.btnBox.TabIndex=2;
            this.btnBox.UseVisualStyleBackColor=true;
            this.btnBox.MouseUp+=new MouseEventHandler(this.btnBox_MouseUp);

            this.btnDots.BackgroundImageLayout=ImageLayout.Stretch;
            this.btnDots.Name="btnDots";
            this.btnDots.Size=new Size(48,48);
            this.btnDots.TabIndex=7;
            this.btnDots.UseVisualStyleBackColor=true;
            this.btnDots.MouseUp+=new MouseEventHandler(this.btnDots_MouseUp);

            this.btnMin.Anchor=AnchorStyles.Top | AnchorStyles.Right;
            this.btnMin.BackgroundImageLayout=ImageLayout.Stretch;
            this.btnMin.Name="btnMin";
            this.btnMin.Size=new Size(27,27);
            this.btnMin.TabIndex=11;
            this.btnMin.UseVisualStyleBackColor=true;
            this.btnMin.Click+=new EventHandler(this.btnMin_Click);

            this.btnExit.Anchor=AnchorStyles.Top | AnchorStyles.Right;
            this.btnExit.BackgroundImageLayout=ImageLayout.Stretch;
            this.btnExit.Name="btnExit";
            this.btnExit.Size=new Size(27,27);
            this.btnExit.TabIndex=12;
            this.btnExit.UseVisualStyleBackColor=true;
            this.btnExit.Click+=new EventHandler(this.btnExit_Click);

            this.btnHintClose.Anchor=AnchorStyles.Top | AnchorStyles.Right;
            this.btnHintClose.BackgroundImageLayout=ImageLayout.Stretch;
            this.btnHintClose.Name="btnHintClose";
            this.btnHintClose.Size=new Size(27,27);
            this.btnHintClose.TabIndex=13;
            this.btnHintClose.UseVisualStyleBackColor=true;
            this.btnHintClose.Click+=new EventHandler(this.btnHintClose_Click);

            this.btnSetClose.BackgroundImageLayout=ImageLayout.Stretch;
            this.btnSetClose.DialogResult=DialogResult.Cancel;
            this.btnSetClose.Name="btnSetClose";
            this.btnSetClose.Size=new Size(27,27);
            this.btnSetClose.TabIndex=14;
            this.btnSetClose.UseVisualStyleBackColor=true;
            this.btnSetClose.Click+=new EventHandler(this.btnSetClose_Click);

            this.btnOK.BackgroundImageLayout=ImageLayout.Stretch;
            this.btnOK.DialogResult=DialogResult.OK;
            this.btnOK.Name="btnOK";
            this.btnOK.Size=new Size(27,27);
            this.btnOK.TabIndex=18;
            this.btnOK.UseVisualStyleBackColor=true;
            this.btnOK.Click+=new EventHandler(this.btnOK_Click);

            this.btnHelp.Anchor=AnchorStyles.Top | AnchorStyles.Right;
            this.btnHelp.BackgroundImageLayout=ImageLayout.Stretch;
            this.btnHelp.Name="btnHelp";
            this.btnHelp.Size=new Size(21,21);
            this.btnHelp.TabIndex=19;
            this.btnHelp.UseVisualStyleBackColor=true;
            this.btnHelp.Click+=new EventHandler(this.btnHelp_Click);
            
            this.btnReset.Anchor=AnchorStyles.Top | AnchorStyles.Right;
            this.btnReset.BackgroundImageLayout=ImageLayout.Stretch;
            this.btnReset.Name="btnReset";
            this.btnReset.Size=new Size(21,21);
            this.btnReset.TabIndex=16;
            this.btnReset.UseVisualStyleBackColor=true;
            this.btnReset.Click+=new EventHandler(this.btnReset_Click);

            this.btnResetColor.Anchor=AnchorStyles.Top | AnchorStyles.Right;
            this.btnResetColor.BackgroundImageLayout=ImageLayout.Stretch;
            this.btnResetColor.Name="btnResetColor";
            this.btnResetColor.Size=new Size(21,21);
            this.btnResetColor.TabIndex=17;
            this.btnResetColor.UseVisualStyleBackColor=true;
            this.btnResetColor.Click+=new EventHandler(this.btnResetColor_Click);

            this.btnSettings.Anchor=AnchorStyles.Top | AnchorStyles.Right;
            this.btnSettings.BackgroundImageLayout=ImageLayout.Stretch;
            this.btnSettings.Name="btnSettings";
            this.btnSettings.Size=new Size(21,21);
            this.btnSettings.TabIndex=15;
            this.btnSettings.UseVisualStyleBackColor=true;
            this.btnSettings.Click+=new EventHandler(this.btnSettings_Click);

            this.btnGhostInfo.Anchor=AnchorStyles.Top | AnchorStyles.Right;
            this.btnGhostInfo.BackgroundImageLayout=ImageLayout.Stretch;
            this.btnGhostInfo.Name="btnGhostInfo";
            this.btnGhostInfo.Size=new Size(21,21);
            this.btnGhostInfo.TabIndex=20;
            this.btnGhostInfo.UseVisualStyleBackColor=true;
            this.btnGhostInfo.Click+=new EventHandler(this.btnGhostInfo_Click);

            //Check Boxes
            this.chbCompact.AutoSize=true;
            this.chbCompact.Font=new Font("Microsoft Sans Serif",10f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.chbCompact.Name="chbCompact";
            this.chbCompact.Size=new Size(81,20);
            this.chbCompact.TabIndex=101;
            this.chbCompact.Text="Compact";
            this.chbCompact.UseVisualStyleBackColor=true;

            this.chbSoundOnOff.AutoSize=true;
            this.chbSoundOnOff.Font=new Font("Microsoft Sans Serif",10f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.chbSoundOnOff.Name="chbSoundOnOff";
            this.chbSoundOnOff.Size=new Size(86,20);
            this.chbSoundOnOff.TabIndex=102;
            this.chbSoundOnOff.Text="Sound On";
            this.chbSoundOnOff.UseVisualStyleBackColor=true;

            //Combo Boxes
            this.cbFirstName.BackColor=Color.FromArgb(1,5,9);
            this.cbFirstName.DropDownStyle=System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cbFirstName.AutoCompleteMode=AutoCompleteMode.None;
            this.cbFirstName.AutoCompleteSource=AutoCompleteSource.None;
            this.cbFirstName.FlatStyle=FlatStyle.Flat;
            this.cbFirstName.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.cbFirstName.ForeColor=Color.WhiteSmoke;
            this.cbFirstName.FormattingEnabled=true;
            this.cbFirstName.Name="cbFirstName";
            this.cbFirstName.Size=new Size(105,23);
            this.cbFirstName.Sorted=true;
            this.cbFirstName.TabIndex=201;
            this.cbFirstName.SelectedIndexChanged+=new EventHandler(this.cbFirstName_SelectedIndexChanged);
            this.cbFirstName.DropDownClosed+=new EventHandler(this.set_focus);

            this.cbLastName.BackColor=Color.FromArgb(1,5,9);
            this.cbLastName.DropDownStyle=System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cbLastName.AutoCompleteMode=AutoCompleteMode.None;
            this.cbLastName.AutoCompleteSource=AutoCompleteSource.None;
            this.cbLastName.FlatStyle=FlatStyle.Flat;
            this.cbLastName.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.cbLastName.ForeColor=Color.WhiteSmoke;
            this.cbLastName.FormattingEnabled=true;
            this.cbLastName.Name="cbLastName";
            this.cbLastName.Size=new Size(105,23);
            this.cbLastName.Sorted=true;
            this.cbLastName.TabIndex=202;
            this.cbLastName.SelectedIndexChanged+=new EventHandler(this.cbLastName_SelectedIndexChanged);
            this.cbLastName.DropDownClosed+=new EventHandler(this.set_focus);

            this.cbAnswerType.Anchor=AnchorStyles.Top | AnchorStyles.Right;
            this.cbAnswerType.BackColor=Color.FromArgb(1,5,9);
            this.cbAnswerType.DropDownStyle=ComboBoxStyle.DropDownList;
            this.cbAnswerType.DropDownWidth=85;
            this.cbAnswerType.FlatStyle=FlatStyle.Flat;
            this.cbAnswerType.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.cbAnswerType.ForeColor=Color.WhiteSmoke;
            this.cbAnswerType.FormattingEnabled=true;
            this.cbAnswerType.ItemHeight=13;
            this.cbAnswerType.Name="cbAnswerType";
            this.cbAnswerType.Size=new Size(85,21);
            this.cbAnswerType.TabIndex=203;
            this.cbAnswerType.SelectedIndexChanged+=new EventHandler(this.cbAnswerType_SelectedIndexChanged);
            this.cbAnswerType.DropDownClosed+=new EventHandler(this.set_focus);
            
            this.cbTask1.Anchor=AnchorStyles.Top | AnchorStyles.Right;
            this.cbTask1.BackColor=Color.FromArgb(1,5,9);
            this.cbTask1.DropDownStyle=ComboBoxStyle.DropDownList;
            this.cbTask1.DropDownWidth=147;
            this.cbTask1.FlatStyle=FlatStyle.Flat;
            this.cbTask1.Font=new Font("Microsoft Sans Serif",9f);
            this.cbTask1.ForeColor=Color.WhiteSmoke;
            this.cbTask1.FormattingEnabled=true;
            this.cbTask1.ItemHeight=13;
            this.cbTask1.Name="cbTask1";
            this.cbTask1.Size=new Size(147,23);
            this.cbTask1.Sorted=true;
            this.cbTask1.TabIndex=204;
            this.cbTask1.SelectedIndexChanged+=new EventHandler(this.cbTask1_SelectedIndexChanged);
            this.cbTask1.DropDownClosed+=new EventHandler(this.set_focus);

            this.cbTask2.Anchor=AnchorStyles.Top | AnchorStyles.Right;
            this.cbTask2.BackColor=Color.FromArgb(1,5,9);
            this.cbTask2.DropDownStyle=ComboBoxStyle.DropDownList;
            this.cbTask2.DropDownWidth=147;
            this.cbTask2.FlatStyle=FlatStyle.Flat;
            this.cbTask2.Font=new Font("Microsoft Sans Serif",9f);
            this.cbTask2.ForeColor=Color.WhiteSmoke;
            this.cbTask2.FormattingEnabled=true;
            this.cbTask2.ItemHeight=13;
            this.cbTask2.Name="cbTask2";
            this.cbTask2.Size=new Size(147,23);
            this.cbTask2.Sorted=true;
            this.cbTask2.TabIndex=205;
            this.cbTask2.SelectedIndexChanged+=new EventHandler(this.cbTask2_SelectedIndexChanged);
            this.cbTask2.DropDownClosed+=new EventHandler(this.set_focus);

            this.cbTask3.Anchor=AnchorStyles.Top | AnchorStyles.Right;
            this.cbTask3.BackColor=Color.FromArgb(1,5,9);
            this.cbTask3.DropDownStyle=ComboBoxStyle.DropDownList;
            this.cbTask3.DropDownWidth=147;
            this.cbTask3.FlatStyle=FlatStyle.Flat;
            this.cbTask3.Font=new Font("Microsoft Sans Serif",9f);
            this.cbTask3.ForeColor=Color.WhiteSmoke;
            this.cbTask3.FormattingEnabled=true;
            this.cbTask3.ItemHeight=14;
            this.cbTask3.Name="cbTask3";
            this.cbTask3.Size=new Size(147,23);
            this.cbTask3.Sorted=true;
            this.cbTask3.TabIndex=206;
            this.cbTask3.SelectedIndexChanged+=new EventHandler(this.cbTask3_SelectedIndexChanged);
            this.cbTask3.DropDownClosed+=new EventHandler(this.set_focus);
            
            this.cbGhosts.BackColor=Color.FromArgb(1,5,9);
            this.cbGhosts.DropDownStyle=ComboBoxStyle.DropDownList;
            this.cbGhosts.DropDownWidth=115;
            this.cbGhosts.FlatStyle=FlatStyle.Flat;
            this.cbGhosts.Font=new Font("Microsoft Sans Serif",9f);
            this.cbGhosts.ForeColor=Color.WhiteSmoke;
            this.cbGhosts.FormattingEnabled=true;
            this.cbGhosts.Name="cbGhosts";
            this.cbGhosts.Size=new Size(115,23);
            this.cbGhosts.Sorted=true;
            this.cbGhosts.TabIndex=207;
            this.cbGhosts.SelectedIndexChanged+=new EventHandler(this.cbGhosts_SelectedIndexChanged);
            this.cbGhosts.DropDownClosed+=new EventHandler(this.set_focus);
            
            this.cbInfo.BackColor=Color.FromArgb(1,5,9);
            this.cbInfo.DropDownStyle=ComboBoxStyle.DropDownList;
            this.cbInfo.DropDownWidth=150;
            this.cbInfo.FlatStyle=FlatStyle.Flat;
            this.cbInfo.Font=new Font("Microsoft Sans Serif",9f);
            this.cbInfo.ForeColor=Color.WhiteSmoke;
            this.cbInfo.FormattingEnabled=true;
            this.cbInfo.Name="cbInfo";
            this.cbInfo.Size=new Size(150,23);
            this.cbInfo.TabIndex=208;
            this.cbInfo.SelectedIndexChanged+=new EventHandler(this.cbInfo_SelectedIndexChanged);
            this.cbInfo.DropDownClosed+=new EventHandler(this.set_focus);

            this.cbTimerType.Anchor=AnchorStyles.Top | AnchorStyles.Right;
            this.cbTimerType.BackColor=Color.FromArgb(1,5,9);
            this.cbTimerType.DropDownStyle=ComboBoxStyle.DropDownList;
            this.cbTimerType.DropDownWidth=107;
            this.cbTimerType.FlatStyle=FlatStyle.Flat;
            this.cbTimerType.Font=new Font("Microsoft Sans Serif",8f);
            this.cbTimerType.ForeColor=Color.WhiteSmoke;
            this.cbTimerType.FormattingEnabled=true;
            this.cbTimerType.ItemHeight=13;
            this.cbTimerType.Name="cbTimerType";
            this.cbTimerType.Size=new Size(107,21);
            this.cbTimerType.Sorted=true;
            this.cbTimerType.TabIndex=209;
            this.cbTimerType.SelectedIndexChanged+=new EventHandler(this.cbTimerType_SelectedIndexChanged);
            this.cbTimerType.DropDownClosed+=new EventHandler(this.set_focus);

            this.cbMaps.Anchor=AnchorStyles.Top | AnchorStyles.Right;
            this.cbMaps.BackColor=Color.FromArgb(1,5,9);
            this.cbMaps.DropDownStyle=ComboBoxStyle.DropDownList;
            this.cbMaps.DropDownWidth=147;
            this.cbMaps.FlatStyle=FlatStyle.Flat;
            this.cbMaps.Font=new Font("Microsoft Sans Serif",8f);
            this.cbMaps.ForeColor=Color.WhiteSmoke;
            this.cbMaps.FormattingEnabled=true;
            this.cbMaps.ItemHeight=13;
            this.cbMaps.Name="cbMaps";
            this.cbMaps.Size=new Size(147,21);
            this.cbMaps.Sorted=true;
            this.cbMaps.TabIndex=210;
            this.cbMaps.SelectedIndexChanged+=new EventHandler(this.cbMaps_SelectedIndexChanged);
            this.cbMaps.DropDownClosed+=new EventHandler(this.set_focus);

            this.cbLanguage.BackColor=Color.FromArgb(1,5,9);
            this.cbLanguage.DropDownStyle=ComboBoxStyle.DropDownList;
            this.cbLanguage.FlatStyle=FlatStyle.Flat;
            this.cbLanguage.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.cbLanguage.ForeColor=Color.WhiteSmoke;
            this.cbLanguage.FormattingEnabled=true;
            this.cbLanguage.Items.AddRange(new object[2] {
                (object)"en",
                (object)"ru"
                });
            this.cbLanguage.Name="cbLanguage";
            this.cbLanguage.Size=new Size(121,23);
            this.cbLanguage.TabIndex=211;
            this.cbLanguage.DropDownClosed+=new EventHandler(this.cbLanguage_SelectedIndexChanged);

            //Labels
            this.lblFocus.AutoSize=true;
            this.lblFocus.Location=new Point(277,19);
            this.lblFocus.Name="lblFocus";
            this.lblFocus.Size=new Size(0,13);
            this.lblFocus.TabIndex=301;
            this.lblFocus.MouseDown+=new MouseEventHandler(this.Main_MouseDown);

            this.lblHint.AutoSize=true;
            this.lblHint.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.lblHint.ForeColor=Color.FromArgb(214,96,96);
            this.lblHint.MaximumSize=new Size(296, 25);
            this.lblHint.Name="lblHint";
            this.lblHint.Size=new Size(0, 15);
            this.lblHint.TabIndex=302;
            this.lblHint.MouseDown+=new MouseEventHandler(this.Main_MouseDown);

            this.lblDif.BackColor=Color.Gray;
            this.lblDif.ForeColor=Color.Black;
            this.lblDif.ImageAlign=ContentAlignment.MiddleLeft;
            this.lblDif.Name="lblDif";
            this.lblDif.TabIndex=303;
            this.lblDif.Text="◉";
            this.lblDif.TextAlign=ContentAlignment.MiddleCenter;
            this.lblDif.Paint+=new PaintEventHandler(this.lblDif_Paint);
            this.lblDif.MouseUp+=new MouseEventHandler(this.lblDif_Click);
            
            this.lblHotKey.AutoSize=true;
            this.lblHotKey.Font=new Font("Microsoft Sans Serif",10f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.lblHotKey.ForeColor=Color.White;
            this.lblHotKey.Name="lblHotKey";
            this.lblHotKey.Size=new Size(52,16);
            this.lblHotKey.TabIndex=304;
            this.lblHotKey.Text="HotKey";
            
            this.lblTimerKey.AutoSize=true;
            this.lblTimerKey.Font=new Font("Microsoft Sans Serif",10f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.lblTimerKey.ForeColor=Color.White;
            this.lblTimerKey.Name="lblTimerKey";
            this.lblTimerKey.Size=new Size(66,16);
            this.lblTimerKey.TabIndex=305;
            this.lblTimerKey.Text="TimerKey";

            this.lblLanguage.AutoSize=true;
            this.lblLanguage.BackColor=Color.Gray;
            this.lblLanguage.Font=new Font("Microsoft Sans Serif",10f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.lblLanguage.ForeColor=Color.White;
            this.lblLanguage.Name="lblLanguage";
            this.lblLanguage.Size=new Size(69,16);
            this.lblLanguage.TabIndex=306;
            this.lblLanguage.Text="Language";
            
            this.lblOpacity.AutoSize=true;
            this.lblOpacity.Font=new Font("Microsoft Sans Serif",10f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.lblOpacity.ForeColor=Color.White;
            this.lblOpacity.Name="lblOpacity";
            this.lblOpacity.Size=new Size(54,16);
            this.lblOpacity.TabIndex=307;
            this.lblOpacity.Text="Opacity";

            this.lblOpacity_Value.AutoSize=true;
            this.lblOpacity_Value.Font=new Font("Microsoft Sans Serif",10f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.lblOpacity_Value.ForeColor=Color.White;
            this.lblOpacity_Value.Name="lblOpacity_Value";
            this.lblOpacity_Value.Size=new Size(43,16);
            this.lblOpacity_Value.TabIndex=308;
            this.lblOpacity_Value.Text="Value";

            this.lblSize.AutoSize=true;
            this.lblSize.Font=new Font("Microsoft Sans Serif",10f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.lblSize.ForeColor=Color.White;
            this.lblSize.Name="lblSize";
            this.lblSize.Size=new Size(34,16);
            this.lblSize.TabIndex=309;
            this.lblSize.Text="Size";

            this.lblSize_Value.AutoSize=true;
            this.lblSize_Value.Font=new Font("Microsoft Sans Serif",10f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.lblSize_Value.ForeColor=Color.White;
            this.lblSize_Value.Name="lblSize_Value";
            this.lblSize_Value.Size=new Size(43,16);
            this.lblSize_Value.TabIndex=310;
            this.lblSize_Value.Text="Value";

            this.lblCopyright.AutoSize=true;
            this.lblCopyright.Font=new Font("Microsoft Sans Serif",10f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.lblCopyright.Name="lblCopyright";
            this.lblCopyright.Size=new Size(78,12);
            this.lblCopyright.TabIndex=311;
            this.lblCopyright.Text="© 2023 by Shaklin";
            
            this.lblVersion.AutoSize=true;
            this.lblVersion.Font=new Font("Microsoft Sans Serif",10f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.lblVersion.Name="lblVersion";
            this.lblVersion.Size=new Size(60,24);
            this.lblVersion.TabIndex=312;
            this.lblVersion.Text="Version 2.1.1\nUpdate by Artiom";

            this.lblTextColor.AutoSize=true;
            this.lblTextColor.Font=new Font("Microsoft Sans Serif",10f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.lblTextColor.ForeColor=Color.White;
            this.lblTextColor.Name="lblTextColor";
            this.lblTextColor.Size=new Size(69,16);
            this.lblTextColor.TabIndex=313;
            this.lblTextColor.Text="Text Color";
            this.lblTextColor.Visible=false;

            this.lblBoxForeColor.AutoSize=true;
            this.lblBoxForeColor.Font=new Font("Microsoft Sans Serif",10f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.lblBoxForeColor.ForeColor=Color.White;
            this.lblBoxForeColor.Name="lblBoxForeColor";
            this.lblBoxForeColor.Size=new Size(95,16);
            this.lblBoxForeColor.TabIndex=314;
            this.lblBoxForeColor.Text="Box Fore Color";
            this.lblBoxForeColor.Visible=false;

            this.lblBoxBackColor.AutoSize=true;
            this.lblBoxBackColor.Font=new Font("Microsoft Sans Serif",10f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.lblBoxBackColor.ForeColor=Color.White;
            this.lblBoxBackColor.Name="lblBoxBackColor";
            this.lblBoxBackColor.Size=new Size(100,16);
            this.lblBoxBackColor.TabIndex=315;
            this.lblBoxBackColor.Text="Box Back Color";
            this.lblBoxBackColor.Visible=false;

            this.lblHintTextColor.AutoSize=true;
            this.lblHintTextColor.Font=new Font("Microsoft Sans Serif",10f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.lblHintTextColor.ForeColor=Color.White;
            this.lblHintTextColor.Name="lblHintTextColor";
            this.lblHintTextColor.Size=new Size(95,16);
            this.lblHintTextColor.TabIndex=316;
            this.lblHintTextColor.Text="Hint Text Color";
            this.lblHintTextColor.Visible=false;

            this.lblBackgroundColor.AutoSize=true;
            this.lblBackgroundColor.Font=new Font("Microsoft Sans Serif",10f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.lblBackgroundColor.ForeColor=Color.White;
            this.lblBackgroundColor.Name="lblBackgroundColor";
            this.lblBackgroundColor.Size=new Size(116,16);
            this.lblBackgroundColor.TabIndex=317;
            this.lblBackgroundColor.Text="Background Color";
            this.lblBackgroundColor.Visible=false;

            this.lblGhInfoType.BackColor=Color.Gray;
            this.lblGhInfoType.BorderStyle=BorderStyle.FixedSingle;
            this.lblGhInfoType.ForeColor=Color.White;
            this.lblGhInfoType.MaximumSize=new Size(355,36);
            this.lblGhInfoType.Name="lblGhInfoType";
            this.lblGhInfoType.Size=new Size(132,36);
            this.lblGhInfoType.TabIndex=318;
            this.lblGhInfoType.TextAlign=ContentAlignment.MiddleCenter;
            this.lblGhInfoType.MouseDown+=new MouseEventHandler(this.Main_MouseDown);
            
            this.lblGhInfoTypeData.BackColor=Color.Gray;
            this.lblGhInfoTypeData.BorderStyle=BorderStyle.FixedSingle;
            this.lblGhInfoTypeData.ForeColor=Color.White;
            this.lblGhInfoTypeData.MaximumSize=new Size(355,36);
            this.lblGhInfoTypeData.Name="lblGhInfoTypeData";
            this.lblGhInfoTypeData.Size=new Size(221,36);
            this.lblGhInfoTypeData.TabIndex=319;
            this.lblGhInfoTypeData.TextAlign=ContentAlignment.MiddleCenter;
            this.lblGhInfoTypeData.MouseDown+=new MouseEventHandler(this.Main_MouseDown);

            this.lblGhInfoHunt.BackColor=Color.Gray;
            this.lblGhInfoHunt.BorderStyle=BorderStyle.FixedSingle;
            this.lblGhInfoHunt.ForeColor=Color.White;
            this.lblGhInfoHunt.MaximumSize=new Size(355,36);
            this.lblGhInfoHunt.Name="lblGhInfoHunt";
            this.lblGhInfoHunt.Size=new Size(132,36);
            this.lblGhInfoHunt.TabIndex=320;
            this.lblGhInfoHunt.TextAlign=ContentAlignment.MiddleCenter;
            this.lblGhInfoHunt.MouseDown+=new MouseEventHandler(this.Main_MouseDown);

            this.lblGhInfoHuntData.BackColor=Color.Gray;
            this.lblGhInfoHuntData.BorderStyle=BorderStyle.FixedSingle;
            this.lblGhInfoHuntData.ForeColor=Color.White;
            this.lblGhInfoHuntData.MaximumSize=new Size(355,36);
            this.lblGhInfoHuntData.Name="lblGhInfoHuntData";
            this.lblGhInfoHuntData.Size=new Size(221,36);
            this.lblGhInfoHuntData.TabIndex=321;
            this.lblGhInfoHuntData.TextAlign=ContentAlignment.MiddleCenter;
            this.lblGhInfoHuntData.MouseDown+=new MouseEventHandler(this.Main_MouseDown);

            this.lblGhInfoHuntAbilities.BackColor=Color.Gray;
            this.lblGhInfoHuntAbilities.BorderStyle=BorderStyle.FixedSingle;
            this.lblGhInfoHuntAbilities.ForeColor=Color.White;
            this.lblGhInfoHuntAbilities.MaximumSize=new Size(355,37);
            this.lblGhInfoHuntAbilities.Name="lblGhInfoHuntAbilities";
            this.lblGhInfoHuntAbilities.Size=new Size(132,37);
            this.lblGhInfoHuntAbilities.TabIndex=323;
            this.lblGhInfoHuntAbilities.TextAlign=ContentAlignment.MiddleCenter;
            this.lblGhInfoHuntAbilities.MouseDown+=new MouseEventHandler(this.Main_MouseDown);

            this.lblGhInfoHuntAbilitiesData.BackColor=Color.Gray;
            this.lblGhInfoHuntAbilitiesData.BorderStyle=BorderStyle.FixedSingle;
            this.lblGhInfoHuntAbilitiesData.ForeColor=Color.White;
            this.lblGhInfoHuntAbilitiesData.MaximumSize=new Size(355,37);
            this.lblGhInfoHuntAbilitiesData.Name="lblGhInfoHuntAbilitiesData";
            this.lblGhInfoHuntAbilitiesData.Size=new Size(221,37);
            this.lblGhInfoHuntAbilitiesData.TabIndex=324;
            this.lblGhInfoHuntAbilitiesData.TextAlign=ContentAlignment.MiddleCenter;
            this.lblGhInfoHuntAbilitiesData.MouseDown+=new MouseEventHandler(this.Main_MouseDown);

            this.lblGhInfoCooldown.BackColor=Color.Gray;
            this.lblGhInfoCooldown.BorderStyle=BorderStyle.FixedSingle;
            this.lblGhInfoCooldown.ForeColor=Color.White;
            this.lblGhInfoCooldown.MaximumSize=new Size(355,20);
            this.lblGhInfoCooldown.Name="lblGhInfoCooldown";
            this.lblGhInfoCooldown.Size=new Size(132,20);
            this.lblGhInfoCooldown.TabIndex=325;
            this.lblGhInfoCooldown.TextAlign=ContentAlignment.MiddleCenter;
            this.lblGhInfoCooldown.MouseDown+=new MouseEventHandler(this.Main_MouseDown);

            this.lblGhInfoCooldownData.BackColor=Color.Gray;
            this.lblGhInfoCooldownData.BorderStyle=BorderStyle.FixedSingle;
            this.lblGhInfoCooldownData.ForeColor=Color.White;
            this.lblGhInfoCooldownData.MaximumSize=new Size(355,20);
            this.lblGhInfoCooldownData.Name="lblGhInfoCooldownData";
            this.lblGhInfoCooldownData.Size=new Size(221,20);
            this.lblGhInfoCooldownData.TabIndex=326;
            this.lblGhInfoCooldownData.TextAlign=ContentAlignment.MiddleCenter;
            this.lblGhInfoCooldownData.MouseDown+=new MouseEventHandler(this.Main_MouseDown);

            //Panel
            this.panel_settings.Cursor=Cursors.Arrow;
            this.panel_settings.Location=new Point(0,0);
            this.panel_settings.Name="panel_settings";
            this.panel_settings.Size=new Size(380,290);
            this.panel_settings.TabIndex=0;
            this.panel_settings.MouseDown+=new MouseEventHandler(this.panel_settings_MouseDown);

            this.panel_settings.Controls.Add((Control)this.btnSetClose);
            this.panel_settings.Controls.Add((Control)this.btnOK);
            this.panel_settings.Controls.Add((Control)this.btnResetColor);
            this.panel_settings.Controls.Add((Control)this.chbCompact);
            this.panel_settings.Controls.Add((Control)this.chbSoundOnOff);
            this.panel_settings.Controls.Add((Control)this.cbLanguage);
            this.panel_settings.Controls.Add((Control)this.lblHotKey);
            this.panel_settings.Controls.Add((Control)this.lblTimerKey);
            this.panel_settings.Controls.Add((Control)this.lblLanguage);
            this.panel_settings.Controls.Add((Control)this.lblOpacity);
            this.panel_settings.Controls.Add((Control)this.lblOpacity_Value);
            this.panel_settings.Controls.Add((Control)this.lblSize);
            this.panel_settings.Controls.Add((Control)this.lblSize_Value);
            this.panel_settings.Controls.Add((Control)this.lblCopyright);
            this.panel_settings.Controls.Add((Control)this.lblVersion);
            this.panel_settings.Controls.Add((Control)this.lblTextColor);
            this.panel_settings.Controls.Add((Control)this.lblBoxForeColor);
            this.panel_settings.Controls.Add((Control)this.lblBoxBackColor);
            this.panel_settings.Controls.Add((Control)this.lblHintTextColor);
            this.panel_settings.Controls.Add((Control)this.lblBackgroundColor);
            this.panel_settings.Controls.Add((Control)this.picGhost);
            this.panel_settings.Controls.Add((Control)this.tbKeyID);
            this.panel_settings.Controls.Add((Control)this.tbTimerID);
            this.panel_settings.Controls.Add((Control)this.tbTextColor_rgb);
            this.panel_settings.Controls.Add((Control)this.tbBoxForeColor_rgb);
            this.panel_settings.Controls.Add((Control)this.tbBoxBackColor_rgb);
            this.panel_settings.Controls.Add((Control)this.tbHintTextColor_rgb);
            this.panel_settings.Controls.Add((Control)this.tbBackgroundColor_rgb);
            this.panel_settings.Controls.Add((Control)this.tbTextColorR);
            this.panel_settings.Controls.Add((Control)this.tbTextColorG);
            this.panel_settings.Controls.Add((Control)this.tbTextColorB);
            this.panel_settings.Controls.Add((Control)this.tbBoxForeColorR);
            this.panel_settings.Controls.Add((Control)this.tbBoxForeColorG);
            this.panel_settings.Controls.Add((Control)this.tbBoxForeColorB);
            this.panel_settings.Controls.Add((Control)this.tbBoxBackColorR);
            this.panel_settings.Controls.Add((Control)this.tbBoxBackColorG);
            this.panel_settings.Controls.Add((Control)this.tbBoxBackColorB);
            this.panel_settings.Controls.Add((Control)this.tbHintTextColorR);
            this.panel_settings.Controls.Add((Control)this.tbHintTextColorG);
            this.panel_settings.Controls.Add((Control)this.tbHintTextColorB);
            this.panel_settings.Controls.Add((Control)this.tbBackgroundColorR);
            this.panel_settings.Controls.Add((Control)this.tbBackgroundColorG);
            this.panel_settings.Controls.Add((Control)this.tbBackgroundColorB);
            this.panel_settings.Controls.Add((Control)this.trbOpacity);
            this.panel_settings.Controls.Add((Control)this.trbSize);
            
            this.panel_settings.ResumeLayout(false);
            this.panel_settings.PerformLayout();

            //Pictures
            this.picLine.BackgroundImageLayout=ImageLayout.Stretch;
            this.picLine.Name="picLine";
            this.picLine.Size=new Size(350,1);
            this.picLine.TabIndex=401;
            this.picLine.TabStop=false;

            this.picGhost.BackgroundImageLayout=ImageLayout.Stretch;
            this.picGhost.Name="picGhost";
            this.picGhost.Size=new Size(32,32);
            this.picGhost.TabIndex=402;
            this.picGhost.TabStop=false;
            this.picGhost.MouseDown+=new MouseEventHandler(this.panel_settings_MouseDown);
            
            this.picGhostEvi1.BackgroundImageLayout=ImageLayout.Stretch;
            this.picGhostEvi1.Name="picGhostEvi1";
            this.picGhostEvi1.Size=new Size(32,32);
            this.picGhostEvi1.TabIndex=403;
            this.picGhostEvi1.TabStop=false;
            this.picGhostEvi1.MouseDown+=new MouseEventHandler(this.Main_MouseDown);
            
            this.picGhostEvi2.BackgroundImageLayout=ImageLayout.Stretch;
            this.picGhostEvi2.Name="picGhostEvi2";
            this.picGhostEvi2.Size=new Size(32,32);
            this.picGhostEvi2.TabIndex=404;
            this.picGhostEvi2.TabStop=false;
            this.picGhostEvi2.MouseDown+=new MouseEventHandler(this.Main_MouseDown);
            
            this.picGhostEvi3.BackgroundImageLayout=ImageLayout.Stretch;
            this.picGhostEvi3.Name="picGhostEvi3";
            this.picGhostEvi3.Size=new Size(32,32);
            this.picGhostEvi3.TabIndex=405;
            this.picGhostEvi3.TabStop=false;
            this.picGhostEvi3.MouseDown+=new MouseEventHandler(this.Main_MouseDown);
            
            this.picGhostEvi4.BackgroundImageLayout=ImageLayout.Stretch;
            this.picGhostEvi4.Name="picGhostEvi4";
            this.picGhostEvi4.Size=new Size(32,32);
            this.picGhostEvi4.TabIndex=406;
            this.picGhostEvi4.TabStop=false;
            this.picGhostEvi4.MouseDown+=new MouseEventHandler(this.Main_MouseDown);

            //Rich Text Boxes
            this.rtbGhostTyp.BackColor=Color.Gray;
            this.rtbGhostTyp.ForeColor=Color.White;
            this.rtbGhostTyp.BorderStyle=BorderStyle.None;
            this.rtbGhostTyp.Cursor=Cursors.Arrow;
            this.rtbGhostTyp.Name="rtbGhostTyp";
            this.rtbGhostTyp.ReadOnly=true;
            this.rtbGhostTyp.ScrollBars=RichTextBoxScrollBars.Vertical;
            this.rtbGhostTyp.ShortcutsEnabled=false;
            this.rtbGhostTyp.Size=new Size(315,65);
            this.rtbGhostTyp.TabIndex=501;
            this.rtbGhostTyp.MouseDown+=new MouseEventHandler(this.Main_MouseDown);
            
            this.rtbGhInfoHintsData.BackColor=Color.FromArgb(1,5,9);
            this.rtbGhInfoHintsData.ForeColor=Color.WhiteSmoke;
            this.rtbGhInfoHintsData.BorderStyle=BorderStyle.None;
            this.rtbGhInfoHintsData.Cursor=Cursors.Arrow;
            this.rtbGhInfoHintsData.Name="rtbGhInfoHintsData";
            this.rtbGhInfoHintsData.ReadOnly=true;
            this.rtbGhInfoHintsData.ScrollBars=RichTextBoxScrollBars.Vertical;
            this.rtbGhInfoHintsData.ShortcutsEnabled=false;
            this.rtbGhInfoHintsData.TabIndex=502;
            this.rtbGhInfoHintsData.Text="";
            this.rtbGhInfoHintsData.MouseDown+=new MouseEventHandler(this.Main_MouseDown);
            
            this.rtbInfo.BackColor=Color.FromArgb(1,5,9);
            this.rtbInfo.ForeColor=Color.WhiteSmoke;
            this.rtbInfo.BorderStyle=BorderStyle.None;
            this.rtbInfo.Cursor=Cursors.Arrow;
            this.rtbInfo.Name="rtbInfo";
            this.rtbInfo.ReadOnly=true;
            this.rtbInfo.ScrollBars=RichTextBoxScrollBars.Vertical;
            this.rtbInfo.ShortcutsEnabled=false;
            this.rtbInfo.TabIndex=503;
            this.rtbInfo.Text="";
            this.rtbInfo.MouseDown+=new MouseEventHandler(this.Main_MouseDown);

            //Text Boxes
            this.tbKeyID.BackColor=Color.WhiteSmoke;
            this.tbKeyID.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.tbKeyID.ForeColor=Color.FromArgb(1,5,9);
            this.tbKeyID.Name="tbKeyID";
            this.tbKeyID.ReadOnly=true;
            this.tbKeyID.Size=new Size(50,21);
            this.tbKeyID.TabIndex=701;
            this.tbKeyID.KeyDown+=new KeyEventHandler(this.tbKeyID_KeyDown);

            this.tbTimerID.BackColor=Color.WhiteSmoke;
            this.tbTimerID.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.tbTimerID.ForeColor=Color.FromArgb(1,5,9);
            this.tbTimerID.Name="tbTimerID";
            this.tbTimerID.ReadOnly=true;
            this.tbTimerID.Size=new Size(50,21);
            this.tbTimerID.TabIndex=702;
            this.tbTimerID.KeyDown+=new KeyEventHandler(this.tbTimerID_KeyDown);

            this.tbCounter.BackColor=Color.White;
            this.tbCounter.ForeColor=SystemColors.InfoText;
            this.tbCounter.BorderStyle=BorderStyle.FixedSingle;
            this.tbCounter.Cursor=Cursors.Arrow;
            this.tbCounter.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.tbCounter.Margin=new Padding(3,3,3,1);
            this.tbCounter.Multiline=true;
            this.tbCounter.Name="tbCounter";
            this.tbCounter.ReadOnly=true;
            this.tbCounter.ShortcutsEnabled=false;
            this.tbCounter.Size=new Size(39,21);
            this.tbCounter.TabIndex=703;
            this.tbCounter.TextAlign=HorizontalAlignment.Center;
            this.tbCounter.KeyDown+=new KeyEventHandler(this.tbCounter_KeyDown);
            this.tbCounter.KeyPress+=new KeyPressEventHandler(this.tbCounter_KeyPress);
            this.tbCounter.KeyUp+=new KeyEventHandler(this.tbCounter_KeyUp);
            this.tbCounter.MouseUp+=new MouseEventHandler(this.tbCounter_MouseUp);

            this.tbTextColor_rgb.BackColor=Color.WhiteSmoke;
            this.tbTextColor_rgb.Cursor=Cursors.Arrow;
            this.tbTextColor_rgb.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.tbTextColor_rgb.Multiline=true;
            this.tbTextColor_rgb.Name="tbTextColor_rgb";
            this.tbTextColor_rgb.ReadOnly=true;
            this.tbTextColor_rgb.ShortcutsEnabled=false;
            this.tbTextColor_rgb.Size=new Size(10,21);
            this.tbTextColor_rgb.TabIndex=704;
            this.tbTextColor_rgb.Visible=false;
            this.tbTextColor_rgb.MouseDown+=new MouseEventHandler(this.Color_rgb_MouseDown);

            this.tbBoxForeColor_rgb.BackColor=Color.WhiteSmoke;
            this.tbBoxForeColor_rgb.Cursor=Cursors.Arrow;
            this.tbBoxForeColor_rgb.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.tbBoxForeColor_rgb.Multiline=true;
            this.tbBoxForeColor_rgb.Name="tbBoxForeColor_rgb";
            this.tbBoxForeColor_rgb.ReadOnly=true;
            this.tbBoxForeColor_rgb.ShortcutsEnabled=false;
            this.tbBoxForeColor_rgb.Size=new Size(10,21);
            this.tbBoxForeColor_rgb.TabIndex=705;
            this.tbBoxForeColor_rgb.Visible=false;
            this.tbBoxForeColor_rgb.MouseDown+=new MouseEventHandler(this.Color_rgb_MouseDown);
            
            this.tbBoxBackColor_rgb.BackColor=Color.WhiteSmoke;
            this.tbBoxBackColor_rgb.Cursor=Cursors.Arrow;
            this.tbBoxBackColor_rgb.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.tbBoxBackColor_rgb.Multiline=true;
            this.tbBoxBackColor_rgb.Name="tbBoxBackColor_rgb";
            this.tbBoxBackColor_rgb.ReadOnly=true;
            this.tbBoxBackColor_rgb.ShortcutsEnabled=false;
            this.tbBoxBackColor_rgb.Size=new Size(10,21);
            this.tbBoxBackColor_rgb.TabIndex=706;
            this.tbBoxBackColor_rgb.Visible=false;
            this.tbBoxBackColor_rgb.MouseDown+=new MouseEventHandler(this.Color_rgb_MouseDown);
            
            this.tbHintTextColor_rgb.BackColor=Color.WhiteSmoke;
            this.tbHintTextColor_rgb.Cursor=Cursors.Arrow;
            this.tbHintTextColor_rgb.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.tbHintTextColor_rgb.Multiline=true;
            this.tbHintTextColor_rgb.Name="tbHintTextColor_rgb";
            this.tbHintTextColor_rgb.ReadOnly=true;
            this.tbHintTextColor_rgb.ShortcutsEnabled=false;
            this.tbHintTextColor_rgb.Size=new Size(10,21);
            this.tbHintTextColor_rgb.TabIndex=707;
            this.tbHintTextColor_rgb.Visible=false;
            this.tbHintTextColor_rgb.MouseDown+=new MouseEventHandler(this.Color_rgb_MouseDown);

            this.tbBackgroundColor_rgb.BackColor=Color.WhiteSmoke;
            this.tbBackgroundColor_rgb.Cursor=Cursors.Arrow;
            this.tbBackgroundColor_rgb.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.tbBackgroundColor_rgb.Multiline=true;
            this.tbBackgroundColor_rgb.Name="tbBackgroundColor_rgb";
            this.tbBackgroundColor_rgb.ReadOnly=true;
            this.tbBackgroundColor_rgb.ShortcutsEnabled=false;
            this.tbBackgroundColor_rgb.Size=new Size(10,21);
            this.tbBackgroundColor_rgb.TabIndex=708;
            this.tbBackgroundColor_rgb.Visible=false;
            this.tbBackgroundColor_rgb.MouseDown+=new MouseEventHandler(this.Color_rgb_MouseDown);

            this.tbTextColorR.BackColor=Color.WhiteSmoke;
            this.tbTextColorR.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.tbTextColorR.Name="tbTextColorR";
            this.tbTextColorR.Size=new Size(30,21);
            this.tbTextColorR.TabIndex=711;
            this.tbTextColorR.Visible=false;
            this.tbTextColorR.KeyPress+=new KeyPressEventHandler(this.Input_OnlyNum);
            this.tbTextColorR.KeyUp+=new KeyEventHandler(this.tbTextColorR_KeyUp);
            this.tbTextColorG.BackColor=Color.WhiteSmoke;
            this.tbTextColorG.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.tbTextColorG.Name="tbTextColorG";
            this.tbTextColorG.Size=new Size(30,21);
            this.tbTextColorG.TabIndex=712;
            this.tbTextColorG.Visible=false;
            this.tbTextColorG.KeyPress+=new KeyPressEventHandler(this.Input_OnlyNum);
            this.tbTextColorG.KeyUp+=new KeyEventHandler(this.tbTextColorG_KeyUp);
            this.tbTextColorB.BackColor=Color.WhiteSmoke;
            this.tbTextColorB.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.tbTextColorB.Name="tbTextColorB";
            this.tbTextColorB.Size=new Size(30,21);
            this.tbTextColorB.TabIndex=713;
            this.tbTextColorB.Visible=false;
            this.tbTextColorB.KeyPress+=new KeyPressEventHandler(this.Input_OnlyNum);
            this.tbTextColorB.KeyUp+=new KeyEventHandler(this.tbTextColorB_KeyUp);

            this.tbBoxForeColorR.BackColor=Color.WhiteSmoke;
            this.tbBoxForeColorR.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.tbBoxForeColorR.Name="tbBoxForeColorR";
            this.tbBoxForeColorR.Size=new Size(30,21);
            this.tbBoxForeColorR.TabIndex=721;
            this.tbBoxForeColorR.Visible=false;
            this.tbBoxForeColorR.KeyPress+=new KeyPressEventHandler(this.Input_OnlyNum);
            this.tbBoxForeColorR.KeyUp+=new KeyEventHandler(this.tbBoxForeColorR_KeyUp);
            this.tbBoxForeColorG.BackColor=Color.WhiteSmoke;
            this.tbBoxForeColorG.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.tbBoxForeColorG.Name="tbBoxForeColorG";
            this.tbBoxForeColorG.Size=new Size(30,21);
            this.tbBoxForeColorG.TabIndex=722;
            this.tbBoxForeColorG.Visible=false;
            this.tbBoxForeColorG.KeyPress+=new KeyPressEventHandler(this.Input_OnlyNum);
            this.tbBoxForeColorG.KeyUp+=new KeyEventHandler(this.tbBoxForeColorG_KeyUp);
            this.tbBoxForeColorB.BackColor=Color.WhiteSmoke;
            this.tbBoxForeColorB.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.tbBoxForeColorB.Name="tbBoxForeColorB";
            this.tbBoxForeColorB.Size=new Size(30,21);
            this.tbBoxForeColorB.TabIndex=723;
            this.tbBoxForeColorB.Visible=false;
            this.tbBoxForeColorB.KeyPress+=new KeyPressEventHandler(this.Input_OnlyNum);
            this.tbBoxForeColorB.KeyUp+=new KeyEventHandler(this.tbBoxForeColorB_KeyUp);
            
            this.tbBoxBackColorR.BackColor=Color.WhiteSmoke;
            this.tbBoxBackColorR.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.tbBoxBackColorR.Name="tbBoxBackColorR";
            this.tbBoxBackColorR.Size=new Size(30,21);
            this.tbBoxBackColorR.TabIndex=731;
            this.tbBoxBackColorR.Visible=false;
            this.tbBoxBackColorR.KeyPress+=new KeyPressEventHandler(this.Input_OnlyNum);
            this.tbBoxBackColorR.KeyUp+=new KeyEventHandler(this.tbBoxBackColorR_KeyUp);
            this.tbBoxBackColorG.BackColor=Color.WhiteSmoke;
            this.tbBoxBackColorG.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.tbBoxBackColorG.Name="tbBoxBackColorG";
            this.tbBoxBackColorG.Size=new Size(30,21);
            this.tbBoxBackColorG.TabIndex=732;
            this.tbBoxBackColorG.Visible=false;
            this.tbBoxBackColorG.KeyPress+=new KeyPressEventHandler(this.Input_OnlyNum);
            this.tbBoxBackColorG.KeyUp+=new KeyEventHandler(this.tbBoxBackColorG_KeyUp);
            this.tbBoxBackColorB.BackColor=Color.WhiteSmoke;
            this.tbBoxBackColorB.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.tbBoxBackColorB.Name="tbBoxBackColorB";
            this.tbBoxBackColorB.Size=new Size(30,21);
            this.tbBoxBackColorB.TabIndex=733;
            this.tbBoxBackColorB.Visible=false;
            this.tbBoxBackColorB.KeyPress+=new KeyPressEventHandler(this.Input_OnlyNum);
            this.tbBoxBackColorB.KeyUp+=new KeyEventHandler(this.tbBoxBackColorB_KeyUp);
            
            this.tbHintTextColorR.BackColor=Color.WhiteSmoke;
            this.tbHintTextColorR.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.tbHintTextColorR.Name="tbHintTextColorR";
            this.tbHintTextColorR.Size=new Size(30,21);
            this.tbHintTextColorR.TabIndex=741;
            this.tbHintTextColorR.Visible=false;
            this.tbHintTextColorR.KeyPress+=new KeyPressEventHandler(this.Input_OnlyNum);
            this.tbHintTextColorR.KeyUp+=new KeyEventHandler(this.tbHintTextColorR_KeyUp);
            this.tbHintTextColorG.BackColor=Color.WhiteSmoke;
            this.tbHintTextColorG.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.tbHintTextColorG.Name="tbHintTextColorG";
            this.tbHintTextColorG.Size=new Size(30,21);
            this.tbHintTextColorG.TabIndex=742;
            this.tbHintTextColorG.Visible=false;
            this.tbHintTextColorG.KeyPress+=new KeyPressEventHandler(this.Input_OnlyNum);
            this.tbHintTextColorG.KeyUp+=new KeyEventHandler(this.tbHintTextColorG_KeyUp);
            this.tbHintTextColorB.BackColor=Color.WhiteSmoke;
            this.tbHintTextColorB.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.tbHintTextColorB.Name="tbHintTextColorB";
            this.tbHintTextColorB.Size=new Size(30,21);
            this.tbHintTextColorB.TabIndex=743;
            this.tbHintTextColorB.Visible=false;
            this.tbHintTextColorB.KeyPress+=new KeyPressEventHandler(this.Input_OnlyNum);
            this.tbHintTextColorB.KeyUp+=new KeyEventHandler(this.tbHintTextColorB_KeyUp);
            
            this.tbBackgroundColorR.BackColor=Color.WhiteSmoke;
            this.tbBackgroundColorR.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.tbBackgroundColorR.Name="tbBackgroundColorR";
            this.tbBackgroundColorR.Size=new Size(30,21);
            this.tbBackgroundColorR.TabIndex=751;
            this.tbBackgroundColorR.Visible=false;
            this.tbBackgroundColorR.KeyPress+=new KeyPressEventHandler(this.Input_OnlyNum);
            this.tbBackgroundColorR.KeyUp+=new KeyEventHandler(this.tbBackgroundColorR_KeyUp);
            this.tbBackgroundColorG.BackColor=Color.WhiteSmoke;
            this.tbBackgroundColorG.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.tbBackgroundColorG.Name="tbBackgroundColorG";
            this.tbBackgroundColorG.Size=new Size(30,21);
            this.tbBackgroundColorG.TabIndex=752;
            this.tbBackgroundColorG.Visible=false;
            this.tbBackgroundColorG.KeyPress+=new KeyPressEventHandler(this.Input_OnlyNum);
            this.tbBackgroundColorG.KeyUp+=new KeyEventHandler(this.tbBackgroundColorG_KeyUp);
            this.tbBackgroundColorB.BackColor=Color.WhiteSmoke;
            this.tbBackgroundColorB.Font=new Font("Microsoft Sans Serif",9f,FontStyle.Regular,GraphicsUnit.Point,(byte)0);
            this.tbBackgroundColorB.Name="tbBackgroundColorB";
            this.tbBackgroundColorB.Size=new Size(30,21);
            this.tbBackgroundColorB.TabIndex=753;
            this.tbBackgroundColorB.Visible=false;
            this.tbBackgroundColorB.KeyPress+=new KeyPressEventHandler(this.Input_OnlyNum);
            this.tbBackgroundColorB.KeyUp+=new KeyEventHandler(this.tbBackgroundColorB_KeyUp);
            
            //Timer
            this.timer.Interval=1000;
            this.timer.Tick+=new EventHandler(this.timer_Tick);

            //Track Bars
            this.trbOpacity.BackColor=Color.FromArgb(1,5,9);
            this.trbOpacity.Maximum=20;
            this.trbOpacity.Minimum=1;
            this.trbOpacity.Name="tbOpacity";
            this.trbOpacity.Size=new Size(161,45);
            this.trbOpacity.TabIndex=801;
            this.trbOpacity.Value=1;
            this.trbOpacity.Scroll+=new EventHandler(this.tbOpacity_Scroll);

            this.trbSize.AutoSize=false;
            this.trbSize.BackColor=Color.FromArgb(1,5,9);
            this.trbSize.Maximum=4;
            this.trbSize.Minimum=1;
            this.trbSize.Name="tbSize";
            this.trbSize.Size=new Size(60,25);
            this.trbSize.TabIndex=802;
            this.trbSize.Value=1;
            this.trbSize.Scroll+=new EventHandler(this.tbSize_Scroll);

            //App
            this.Controls.Add((Control)this.panel_settings);
            this.Controls.Add((Control)this.lblFocus);
            this.Controls.Add((Control)this.lblHint);
            this.Controls.Add((Control)this.lblDif);
            this.Controls.Add((Control)this.lblGhInfoType);
            this.Controls.Add((Control)this.lblGhInfoTypeData);
            this.Controls.Add((Control)this.lblGhInfoHunt);
            this.Controls.Add((Control)this.lblGhInfoHuntData);
            this.Controls.Add((Control)this.lblGhInfoHuntAbilities);
            this.Controls.Add((Control)this.lblGhInfoHuntAbilitiesData);
            this.Controls.Add((Control)this.lblGhInfoCooldown);
            this.Controls.Add((Control)this.lblGhInfoCooldownData);
            this.Controls.Add((Control)this.picLine);
            this.Controls.Add((Control)this.picGhostEvi1);
            this.Controls.Add((Control)this.picGhostEvi2);
            this.Controls.Add((Control)this.picGhostEvi3);
            this.Controls.Add((Control)this.picGhostEvi4);
            this.Controls.Add((Control)this.btnEMF);
            this.Controls.Add((Control)this.btnFinger);
            this.Controls.Add((Control)this.btnBook);
            this.Controls.Add((Control)this.btnTemp);
            this.Controls.Add((Control)this.btnOrb);
            this.Controls.Add((Control)this.btnBox);
            this.Controls.Add((Control)this.btnDots);
            this.Controls.Add((Control)this.btnMin);
            this.Controls.Add((Control)this.btnExit);
            this.Controls.Add((Control)this.btnHintClose);
            this.Controls.Add((Control)this.btnHelp);
            this.Controls.Add((Control)this.btnReset);
            this.Controls.Add((Control)this.btnSettings);
            this.Controls.Add((Control)this.btnGhostInfo);
            this.Controls.Add((Control)this.cbFirstName);
            this.Controls.Add((Control)this.cbLastName);
            this.Controls.Add((Control)this.cbAnswerType);
            this.Controls.Add((Control)this.cbTask1);
            this.Controls.Add((Control)this.cbTask2);
            this.Controls.Add((Control)this.cbTask3);
            this.Controls.Add((Control)this.cbGhosts);
            this.Controls.Add((Control)this.cbInfo);
            this.Controls.Add((Control)this.cbTimerType);
            this.Controls.Add((Control)this.cbMaps);
            this.Controls.Add((Control)this.rtbGhostTyp);
            this.Controls.Add((Control)this.rtbGhInfoHintsData);
            this.Controls.Add((Control)this.rtbInfo);
            this.Controls.Add((Control)this.tbCounter);

            this.ForeColor=Color.WhiteSmoke;
            this.FormBorderStyle=FormBorderStyle.None;
            this.Icon=(Icon)PhasmoGadgetRes.Icon;
            this.KeyPreview=true;
            this.Name="PhasmoGadget";
            this.StartPosition=FormStartPosition.Manual;
            this.Text="Phasmo Gadget";
            this.TopMost=true;
            this.TransparencyKey=Color.Gray;
            this.Load+=new EventHandler(this.Main_Load);
            this.MouseDown+=new MouseEventHandler(this.Main_MouseDown);

            this.trbSize.EndInit();
            this.trbOpacity.EndInit();
            ((ISupportInitialize)this.picLine).EndInit();
            ((ISupportInitialize)this.picGhost).EndInit();
            ((ISupportInitialize)this.picGhostEvi1).EndInit();
            ((ISupportInitialize)this.picGhostEvi2).EndInit();
            ((ISupportInitialize)this.picGhostEvi3).EndInit();
            ((ISupportInitialize)this.picGhostEvi4).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}