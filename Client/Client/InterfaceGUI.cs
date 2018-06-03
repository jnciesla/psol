﻿using GeonBit.UI.Entities;
using GeonBit.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeonBit.UI.Utils;

namespace Client
{
    class InterfaceGUI
    {
        public static List<Panel> Windows = new List<Panel>();
        private ClientTCP ctcp = new ClientTCP();

        public MulticolorParagraph lblStatus;
        public TextInput txtUser;
        public TextInput txtPass;
        public TextInput txtUserReg;
        public TextInput txtPassReg;
        public TextInput txtPas2Reg;
        public Button btnLogin;

        private string mask = "*";

        public void InitializeGUI()
        {
            CreateWindow_Login();
            CreateWindow_Register();
            // UserInterface.Active.GlobalScale = .75F;
        }

        public void CreateWindow(Panel panel)
        {
            Windows.Add(panel);
        }

        public void TabThrough()
        {
            if (Windows[0].Visible == true) // Tab through login window when visible
            {
                if (txtUser.IsFocused == true)
                {
                    txtUser.IsFocused = false;
                    txtPass.IsFocused = true;
                }
                else
                {
                    txtUser.IsFocused = true;
                    txtPass.IsFocused = false;
                }
            }

            if (Windows[1].Visible == true) // Tab through register window when visible
            {
                if (txtUserReg.IsFocused == true)
                {
                    txtUserReg.IsFocused = false;
                    txtPassReg.IsFocused = true;
                }
                else if (txtPassReg.IsFocused == true)
                {
                    txtPas2Reg.IsFocused = true;
                    txtPassReg.IsFocused = false;
                }
                else
                {
                    txtPas2Reg.IsFocused = false;
                    txtUserReg.IsFocused = true;
                }
            }

        }

        public void Enter()
        {
            if (Windows[0].Visible == true) // Enter on login window when visible
            {
                Login();
            }
            if (Windows[1].Visible == true) // Enter on register window when visible
            {
                Register();
            }
        }

        private void Login()
        {
            if (Globals.loginUsername == string.Empty || Globals.loginPassword == string.Empty)
            {
                MessageBox.ShowMsgBox("No credentials", "Please enter a valid username and password before logging in!", new MessageBox.MsgBoxOption[]
                {
                        new MessageBox.MsgBoxOption("Okay" ,() => {return true; })
                });
            }
            else
            {
                ctcp.SendLogin();
            }
        }

        private void Register()
        {
            if (Globals.registerUsername == string.Empty || Globals.registerPassword == string.Empty || Globals.registerValidate == string.Empty)
            {
                MessageBox.ShowMsgBox("No credentials", "Please enter a valid username and password, and confirm your password, before attempting to register!", new MessageBox.MsgBoxOption[]
                {
                        new MessageBox.MsgBoxOption("Okay" ,() => {return true; })
                });
            }
            else
            {
                if (Globals.registerPassword != Globals.registerValidate)
                {
                    MessageBox.ShowMsgBox("Passwords do not match", "The passwords you have entered to not match.  Please try again.", new MessageBox.MsgBoxOption[]
                    {
                        new MessageBox.MsgBoxOption("Okay" ,() => {return true; })
                    });
                }
                else
                {
                    Globals.loginUsername = Globals.registerUsername;
                    Globals.loginPassword = Globals.registerPassword;
                    ctcp.SendRegister();
                }
            }
        }

        public void CreateWindow_Login()
        {
            //  Create Entities
            Panel panel = new Panel(new Vector2(500, 430));
            btnLogin = new Button("Login");
            txtUser = new TextInput(false);
            txtUser.Validators.Add(new Validators.AlphaNumValidator());
            Header headerUser = new Header("Username", Anchor.TopCenter);
            txtPass = new TextInput(false);
            txtPass.HideInputWithChar = mask.ToCharArray()[0];
            txtPass.Validators.Add(new Validators.AlphaNumValidator());
            Header headerPass = new Header("Password", Anchor.AutoCenter);
            Label lblRegister = new Label("No account?  Register here", Anchor.AutoCenter);
            lblStatus = new MulticolorParagraph("Server status:{{RED}} offline", Anchor.BottomLeft);
            UserInterface.Active.AddEntity(panel);

            // Entity Settings
            txtUser.PlaceholderText = "Enter username";
            txtPass.PlaceholderText = "Enter password";

            // Add Entities
            panel.AddChild(headerUser);
            panel.AddChild(txtUser);
            panel.AddChild(headerPass);
            panel.AddChild(txtPass);
            panel.AddChild(btnLogin);
            panel.AddChild(lblRegister);
            panel.AddChild(lblStatus);

            // MouseEvents
            lblRegister.OnMouseEnter += (Entity entity) => { lblRegister.FillColor = Color.Red; UserInterface.Active.SetCursor(CursorType.Pointer); };
            lblRegister.OnMouseLeave += (Entity entity) => { lblRegister.FillColor = Color.White; UserInterface.Active.SetCursor(CursorType.Default); };

            txtUser.OnMouseEnter += (Entity entity) => { UserInterface.Active.SetCursor(CursorType.IBeam); };
            txtUser.OnMouseLeave += (Entity entity) => { UserInterface.Active.SetCursor(CursorType.Default); };

            txtPass.OnMouseEnter += (Entity entity) => { UserInterface.Active.SetCursor(CursorType.IBeam); };
            txtPass.OnMouseLeave += (Entity entity) => { UserInterface.Active.SetCursor(CursorType.Default); };

            btnLogin.OnMouseEnter += (Entity entity) => { UserInterface.Active.SetCursor(CursorType.Pointer); };
            btnLogin.OnMouseLeave += (Entity entity) => { UserInterface.Active.SetCursor(CursorType.Default); };

            lblRegister.OnClick += (Entity entity) =>
            {
                MenuManager.ChangeMenu(MenuManager.Menu.Register);
            };

            btnLogin.OnClick += (Entity entity) =>
            {
                Login();
            };

            txtUser.OnValueChange = (Entity textUser) => { Globals.loginUsername = txtUser.Value; };
            txtPass.OnValueChange = (Entity textPass) => { Globals.loginPassword = txtPass.Value; };

            // Create Window
            CreateWindow(panel);
        }

        public void CreateWindow_Register()
        {
            //  Create Entities
            Panel panel = new Panel(new Vector2(500, 550));
            Button btnRegister = new Button("Register");
            Button btnBack = new Button("Back");
            txtUserReg = new TextInput(false);
            txtUserReg.Validators.Add(new Validators.AlphaNumValidator());
            Header headerUser = new Header("Username", Anchor.TopCenter);
            txtPassReg = new TextInput(false);
            txtPassReg.Validators.Add(new Validators.AlphaNumValidator());
            txtPassReg.HideInputWithChar = mask.ToCharArray()[0];
            Header headerPass = new Header("Password", Anchor.AutoCenter);
            txtPas2Reg = new TextInput(false);
            txtPas2Reg.Validators.Add(new Validators.AlphaNumValidator());
            txtPas2Reg.HideInputWithChar = mask.ToCharArray()[0];
            Header headerPass2 = new Header("Confirm password", Anchor.AutoCenter);
            UserInterface.Active.AddEntity(panel);

            // Entity Settings
            txtUserReg.PlaceholderText = "Enter username";
            txtPassReg.PlaceholderText = "Enter password";
            txtPas2Reg.PlaceholderText = "Confirm password";

            // Add Entities
            panel.AddChild(headerUser);
            panel.AddChild(txtUserReg);
            panel.AddChild(headerPass);
            panel.AddChild(txtPassReg);
            panel.AddChild(headerPass2);
            panel.AddChild(txtPas2Reg);
            panel.AddChild(btnRegister);
            panel.AddChild(btnBack);

            // MouseEvents
            txtUserReg.OnMouseEnter += (Entity entity) => { UserInterface.Active.SetCursor(CursorType.IBeam); };
            txtUserReg.OnMouseLeave += (Entity entity) => { UserInterface.Active.SetCursor(CursorType.Default); };

            txtPassReg.OnMouseEnter += (Entity entity) => { UserInterface.Active.SetCursor(CursorType.IBeam); };
            txtPassReg.OnMouseLeave += (Entity entity) => { UserInterface.Active.SetCursor(CursorType.Default); };

            txtPas2Reg.OnMouseEnter += (Entity entity) => { UserInterface.Active.SetCursor(CursorType.IBeam); };
            txtPas2Reg.OnMouseLeave += (Entity entity) => { UserInterface.Active.SetCursor(CursorType.Default); };

            btnRegister.OnMouseEnter += (Entity entity) => { UserInterface.Active.SetCursor(CursorType.Pointer); };
            btnRegister.OnMouseLeave += (Entity entity) => { UserInterface.Active.SetCursor(CursorType.Default); };

            btnBack.OnMouseEnter += (Entity entity) => { UserInterface.Active.SetCursor(CursorType.Pointer); };
            btnBack.OnMouseLeave += (Entity entity) => { UserInterface.Active.SetCursor(CursorType.Default); };

            btnBack.OnClick += (Entity entity) =>
            {
                MenuManager.ChangeMenu(MenuManager.Menu.Login);
            };
            btnRegister.OnClick += (Entity entity) =>
            {
                Register();
            };

            txtUserReg.OnValueChange = (Entity textUserReg) => { Globals.registerUsername = txtUserReg.Value; };
            txtPassReg.OnValueChange = (Entity textPassReg) => { Globals.registerPassword = txtPassReg.Value; };
            txtPas2Reg.OnValueChange = (Entity textPas2Reg) => { Globals.registerValidate = txtPas2Reg.Value; };

            // Create Window
            CreateWindow(panel);
        }
    }
}
