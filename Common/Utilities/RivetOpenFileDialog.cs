// ===========================================================================================================
//  Common Public Attribution License Version 1.0.
//
//  The contents of this file are subject to the Common Public Attribution License Version 1.0 (the “License”); 
//  you may not use this file except in compliance with the License. You may obtain a copy of the License at
//  http://www.rivetsoftware.com/content/index.cfm?fuseaction=showContent&contentID=212&navID=180.
//
//  The License is based on the Mozilla Public License Version 1.1 but Sections 14 and 15 have been added to 
//  cover use of software over a computer network and provide for limited attribution for the Original Developer. 
//  In addition, Exhibit A has been modified to be consistent with Exhibit B.
//
//  Software distributed under the License is distributed on an “AS IS” basis, WITHOUT WARRANTY OF ANY KIND, 
//  either express or implied. See the License for the specific language governing rights and limitations 
//  under the License.
//
//  The Original Code is Rivet Dragon Tag XBRL Enabler.
//
//  The Initial Developer of the Original Code is Rivet Software, Inc.. All portions of the code written by 
//  Rivet Software, Inc. are Copyright (c) 2004-2008. All Rights Reserved.
//
//  Contributor: Rivet Software, Inc..
// ===========================================================================================================
using System;
using System.Text;

using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Aucent.MAX.AXE.Common.Utilities
{
	/// <summary>
	/// Summary description for RivetOpenFileDialog.
	/// </summary>
	public class RivetOpenFileDialog 
	{
		#region Win32 Imports
		[ DllImport( "Comdlg32.dll", CharSet=CharSet.Auto )]
		protected static extern bool GetOpenFileName([ In, Out ] OpenFileName ofn );  

		[DllImport("user32.dll")] 
		protected static extern IntPtr GetParent( IntPtr childHwnd );

		[DllImport("user32.dll", CharSet=CharSet.Auto )] 
		protected static extern int SendMessage( IntPtr hwnd, int msg, int wParam, StringBuilder lParam );

		[DllImport("comdlg32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
		public static extern int CommDlgExtendedError();
 
		#endregion

		#region delegates
		protected delegate IntPtr HookProc( IntPtr dlgHandle, uint msg, IntPtr wParam, IntPtr lParam );
		#endregion

		#region constants
		protected const uint	OFN_EXPLORER		= 0x00080000;     // new look commdlg
		protected const uint	OFN_ENABLEHOOK		= 0x00000020;
		protected const	uint	OFN_FILEMUSTEXIST	= 0x00001000;
		protected const uint	OFN_PATHMUSTEXIST   = 0x00000800;
		protected const uint	OFN_NOCHANGEDIR     = 0x00000008;
		protected const uint	OFN_HIDEREADONLY    = 0x00000004;
		protected const uint	OFN_ALLOWMULTISELECT= 0x00000200;

		protected const int		CDN_FIRST			= -601;
		protected const int		CDN_FILEOK			= (CDN_FIRST - 0x0005);

		protected const int		WM_USER				= 0x400;
		protected const int		CDM_FIRST			= (WM_USER + 100);

		protected const int		CDM_GETSPEC			= (CDM_FIRST + 0x0000);
		protected const int		CDM_GETFILEPATH		= (CDM_FIRST + 0x0001);
		protected const int		CDM_GETFOLDERPATH	= (CDM_FIRST + 0x0002);

		protected const int		WM_NOTIFY			= 0x004E;
		#endregion

		#region internal classes
		[ StructLayout( LayoutKind.Sequential, CharSet=CharSet.Auto )]  
			protected class NMHDR 
		{
			public IntPtr hwndFrom;
			public IntPtr idFrom;
			public int code;
		};

		[ StructLayout( LayoutKind.Sequential, CharSet=CharSet.Auto )]  
			protected class OFNotify
		{
			public NMHDR header = null;
			public IntPtr ofn;
			public IntPtr file;
		}
	
		[ StructLayout( LayoutKind.Sequential, CharSet=CharSet.Auto )]  
			protected class OpenFileName 
		{
			public int				StructSize = 0;
			public int				HwndOwner = 0;
			public int				HInstance = 0;
			public string			Filter = null;
			public string			CustomFilter = null;
			public int				MaxCustFilter = 0;
			public int				FilterIndex = 0;
			public string			File = null;
			public int				MaxFile = 0;
			public string			FileTitle = null;
			public int				MaxFileTitle = 0;
			public string			InitialDirectory = null;
			public string			Title = null;
			public uint				Flags = 0;
			public short			FileOffset = 0;
			public short			FileExtension = 0;
			public string			DefaultExt = null;
			public IntPtr			CustData;
			public HookProc			Hook = null;
			public string			TemplateName = null;
			public IntPtr			Reserved;
			public uint				Reserved2;
			public uint				FlagsEx;
		};
		#endregion

		#region local variables
		protected OpenFileName  ofn = new OpenFileName();
		protected StringBuilder originalLocation = new StringBuilder( 1024 );
		protected string		currentFile = null;
		#endregion

		#region accessors
		public string FileName
		{
			get { return currentFile; }
			set { currentFile = value; }
		}

		//		public int FileLength
		//		{
		//			get { return ofn.MaxFile; }
		//			set
		//			{
		//				ofn.File = new StringBuilder( value );
		//				ofn.MaxFile = ofn.File.Length;
		//			}
		//		}

		public string Filter
		{
			get { return ofn.Filter; }
			set { ofn.Filter = value; }
		}

		public int FilterIndex
		{
			get { return ofn.FilterIndex; }
			set { ofn.FilterIndex = value; }
		}

		public string InitialDirectory
		{
			get { return ofn.InitialDirectory; }
			set { ofn.InitialDirectory = value; }
		}

		public string Title
		{
			get { return ofn.Title; }
			set { ofn.Title = value; }
		}

		public string DefaultExtension
		{
			get { return ofn.DefaultExt; }
			set { ofn.DefaultExt = value; }
		}

		public string OriginalPath
		{
			get { return originalLocation.ToString(); }
		}

		public bool CheckFileExists
		{
			get { return GetOption( OFN_FILEMUSTEXIST ); }
			set { SetOption( OFN_FILEMUSTEXIST, value ); }
		}

		public bool CheckPathExists
		{
			get { return GetOption( OFN_PATHMUSTEXIST ); }
			set { SetOption( OFN_PATHMUSTEXIST, value ); }
		}

		public bool RestoreDirectory
		{
			get { return GetOption( OFN_NOCHANGEDIR ); }
			set { SetOption( OFN_NOCHANGEDIR, value ); }
		}

		public bool Multiselect
		{
			get { return GetOption( OFN_ALLOWMULTISELECT ); }
			set { SetOption( OFN_ALLOWMULTISELECT, value ); }
		}

		public bool HideReadOnly
		{
			get { return GetOption( OFN_HIDEREADONLY ); }
			set { SetOption( OFN_HIDEREADONLY, value ); }
		}

		protected bool GetOption( uint option )
		{
			return ( ofn.Flags & option ) != 0;
		}

		protected void SetOption( uint option, bool value )
		{
			if ( value )
			{
				ofn.Flags |= option;
			}
			else
			{
				ofn.Flags &= ~option;
			}
		}
		#endregion

		public RivetOpenFileDialog()
		{
			//			ofn.StructSize	= Marshal.SizeOf( typeof( OpenFileName ) );
			ofn.Flags		= OFN_EXPLORER + OFN_ENABLEHOOK;
			ofn.Hook		= new HookProc( OFNHook );
			//			ofn.MaxFile		= 0x2000;
			//			ofn.File		= new StringBuilder( ofn.MaxFile );
		}

		protected IntPtr OFNHook( IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam )
		{
			// this is undocumented, but seems to work. maybe we should wait for the file to be 
			// downloaded (which is the commented out else clause 
			//			if ( msg == 0x1f )
			//			{
			//				IntPtr parent = GetParent( hwnd );
			//				SendMessage( parent, CDM_GETSPEC, originalLocation.Capacity, originalLocation );
			//
			//				return new IntPtr( 1 );
			//			}
			if	( msg == WM_NOTIFY )
			{
				OFNotify notify = (OFNotify)Marshal.PtrToStructure( lParam, typeof( OFNotify ) );

				if ( notify != null && notify.header.code == CDN_FILEOK ) 
				{
					SendMessage( notify.header.hwndFrom, CDM_GETSPEC, originalLocation.Capacity, originalLocation );
				}
			}

			return IntPtr.Zero;
		}

		public DialogResult ShowDialog()
		{
			return ShowDialog( null );
		}

		public DialogResult ShowDialog( IWin32Window handle )
		{
			ofn.HwndOwner = handle.Handle.ToInt32();

			ofn.StructSize	= Marshal.SizeOf( typeof( OpenFileName ) );

			char[] fn = new char[ 0x2000 ];
			if ( currentFile != null )
			{
				currentFile.CopyTo( 0, fn, 0, currentFile.Length );
			}

			ofn.File = new string( new char[ 0x2000 ] );
			ofn.MaxFile = 0x2000;
			

			if ( GetOpenFileName( ofn ) )
			{
				currentFile = ofn.File;

				return DialogResult.OK;
			}
			else
			{
				switch ( CommDlgExtendedError() )
				{
					case 0x3001:
						throw new InvalidOperationException( "RivetOpenFileDialog: Subclass failure" );

					case 12290:
						throw new InvalidOperationException( "RivetOpenFileDialog: InvalidFilename: " + FileName ); 

					case 0x3003:
						throw new InvalidOperationException( "RivetOpenFileDialog: FileDialogBuffer Too small" ); 
				}
			}

			return DialogResult.Cancel;
		}
	}
}
