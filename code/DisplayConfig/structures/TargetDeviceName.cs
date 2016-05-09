﻿using System;
using System.Runtime.InteropServices;


namespace ManagedX.Display.DisplayConfig
{
	using Graphics;
	using Win32;


	/// <summary>Contains information about the target (defined in WinGDI.h).</summary>
	/// <remarks>https://msdn.microsoft.com/en-us/library/windows/hardware/ff553989%28v=vs.85%29.aspx</remarks>
	[System.Diagnostics.DebuggerStepThrough]
	[Native( "WinGDI.h", "DISPLAYCONFIG_TARGET_DEVICE_NAME" )]
	[StructLayout( LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 2, Size = 420 )]
	public struct TargetDeviceName : IEquatable<TargetDeviceName>
	{

		/// <summary>Defines the maximum length, in chars, of the <see cref="FriendlyName"/>.</summary>
		public const int MaxFriendlyNameLength = 64;

		/// <summary>Defines the maximum length, in chars, of the <see cref="DevicePath"/>.</summary>
		public const int MaxDevicePathLength = 128;


		/// <remarks>https://msdn.microsoft.com/en-us/library/windows/hardware/ff553990%28v=vs.85%29.aspx</remarks>
		[Native( "WinGDI.h", "DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS" )]
		[Flags]
		private enum Indicators : int
		{

			None = 0x00000000,

			/// <summary>The string in the monitorFriendlyDeviceName member of the <see cref="TargetDeviceName"/> structure was constructed
			/// from the manufacture identification string in the extended display identification data (EDID).
			/// </summary>
			[Native( "WinGDI.h", "friendlyNameFromEdid" )]
			FriendlyNameFromExtendedDisplayInformationData = 0x00000001,

			/// <summary>The target is forced with no detectable monitor attached and the monitorFriendlyDeviceName member of the
			/// <see cref="TargetDeviceName"/> structure is a null-terminated empty string.
			/// </summary>
			[Native( "WinGDI.h", "friendlyNameForced" )]
			FriendlyNameForced = 0x00000002,

			/// <summary>The edidManufactureId and edidProductCodeId members of the <see cref="TargetDeviceName"/> structure are valid and
			/// were obtained from the extended display information data (EDID).
			/// </summary>
			[Native( "WinGDI.h", "edidIdsValid" )]
			ExtendedDisplayInformationDataIdsValid = 0x00000004

		}



		/// <summary>A <see cref="DeviceInfoHeader"/> structure that contains information about the request for the target device name.
		/// The caller should set the <code>type</code> member of <see cref="DeviceInfoHeader"/> to <code><see cref="DeviceInfoType"/>.GetTargetName</code> and the <code>adapterId</code> and <code>id</code> members of <see cref="DeviceInfoHeader"/> to the target for which the caller wants the target device name.
		/// The caller should set the <code>size</code> member of <see cref="DeviceInfoHeader"/> to at least the size of the <see cref="TargetDeviceName"/> structure.</summary>
		private DeviceInfoHeader header;
		private Indicators indicators;
		private VideoOutputTechnology outputTechnology;
		private short edidManufactureId;	// might not be valid (see flags)
		private short edidProductCodeId;	// might not be valid (see flags)
		private int connectorInstance;
		[MarshalAs( UnmanagedType.ByValTStr, SizeConst = MaxFriendlyNameLength )]
		private string monitorFriendlyDeviceName; // might not be valid (see flags)
		[MarshalAs( UnmanagedType.ByValTStr, SizeConst = MaxDevicePathLength )]
		private string monitorDevicePath;


		/// <summary>Instantiates a new <see cref="TargetDeviceName"/> structure.</summary>
		/// <param name="adapterId">The adapter identifier.</param>
		/// <param name="targetId">The target device identifier.</param>
		public TargetDeviceName( Luid adapterId, int targetId )
			: this()
		{
			header = new DeviceInfoHeader( DeviceInfoType.GetTargetName, Marshal.SizeOf( typeof( TargetDeviceName ) ), adapterId, targetId );
			monitorDevicePath = monitorFriendlyDeviceName = string.Empty;
		}


		/// <summary>Gets the identifier of the target adapter device the information packet refers to.</summary>
		public Luid AdapterId { get { return header.AdapterId; } }


		/// <summary>Gets the identifier of the target adapter device to get or set the information for.</summary>
		public int TargetId { get { return header.Id; } }


		/// <summary>A value from the <see cref="VideoOutputTechnology"/> enumeration that specifies the target's connector type.</summary>
		public VideoOutputTechnology OutputTechnology { get { return outputTechnology; } }


		#region EDID

		/// <summary>Gets a value indicating whether extended display identification data (EDID) is valid; see <see cref="ExtendedDisplayIdentificationDataManufactureId"/> and <see cref="ExtendedDisplayIdentificationDataProductCodeId"/>.</summary>
		public bool IsExtendedDisplayIdentificationDataValid { get { return ( indicators & Indicators.ExtendedDisplayInformationDataIdsValid ) == Indicators.ExtendedDisplayInformationDataIdsValid; } }


		/// <summary>When <see cref="IsExtendedDisplayIdentificationDataValid"/> is true, gets the manufacture identifier from the monitor extended display identification data (EDID).</summary>
		public short ExtendedDisplayIdentificationDataManufactureId
		{
			get
			{
				if( indicators.HasFlag( Indicators.ExtendedDisplayInformationDataIdsValid ) )
					return edidManufactureId;
				return 0;
			}
		}


		/// <summary>When <see cref="IsExtendedDisplayIdentificationDataValid"/> is true, gets the product code from the monitor extended display identification data (EDID).</summary>
		public short ExtendedDisplayIdentificationDataProductCodeId
		{
			get
			{
				if( indicators.HasFlag( Indicators.ExtendedDisplayInformationDataIdsValid ) )
					return edidProductCodeId;
				return 0;
			}
		}

		#endregion EDID


		/// <summary>The one-based instance number of this particular target only when the adapter has multiple targets of this type.
		/// The connector instance is a consecutive one-based number that is unique within each adapter.
		/// If this is the only target of this type on the adapter, this value is zero.
		/// </summary>
		public int ConnectorInstance { get { return connectorInstance; } }


		/// <summary>Gets the friendly name for the monitor.
		/// <para>This name can be used with SetupAPI.dll to obtain the device name that is contained in the installation package.</para>
		/// </summary>
		public string FriendlyName
		{
			get
			{
				if( indicators.HasFlag( Indicators.FriendlyNameForced ) )
					return "Unknown";

				if( indicators.HasFlag( Indicators.FriendlyNameFromExtendedDisplayInformationData ) && !string.IsNullOrWhiteSpace( monitorFriendlyDeviceName ) )
					return string.Copy( monitorFriendlyDeviceName );

				return "Generic PnP Monitor";
			}
		}


		/// <summary>A (unicode) string that is the path to the device name for the monitor.
		/// <para>This path can be used with SetupAPI.dll to obtain the device name that is contained in the installation package.</para>
		/// </summary>
		public string DevicePath { get { return string.Copy( monitorDevicePath ?? string.Empty ); } }


		/// <summary>Returns a hash code for this <see cref="TargetDeviceName"/> structure.</summary>
		/// <returns>Returns a hash code for this <see cref="TargetDeviceName"/> structure.</returns>
		public override int GetHashCode()
		{
			return header.GetHashCode() ^ (int)indicators ^ (int)outputTechnology ^ connectorInstance ^ ( monitorDevicePath ?? string.Empty ).GetHashCode();
		}


		/// <summary></summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals( TargetDeviceName other )
		{
			return header.Equals( other.header ) && ( indicators == other.indicators ) && ( outputTechnology == other.outputTechnology ) && ( connectorInstance == other.connectorInstance ) && this.DevicePath.Equals( other.DevicePath );
		}


		/// <summary></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals( object obj )
		{
			return ( obj is TargetDeviceName ) && this.Equals( (TargetDeviceName)obj );
		}


		/// <summary>Returns the <see cref="FriendlyName"/>.</summary>
		/// <returns>Returns the <see cref="FriendlyName"/>.</returns>
		public override string ToString()
		{
			return this.FriendlyName;
		}


		#region Operators


		/// <summary>Equality operator.</summary>
		/// <param name="targetDeviceName">A <see cref="TargetDeviceName"/> structure.</param>
		/// <param name="other">A <see cref="TargetDeviceName"/> structure.</param>
		/// <returns>Returns true if the structures are equal, otherwise returns false.</returns>
		public static bool operator ==( TargetDeviceName targetDeviceName, TargetDeviceName other )
		{
			return targetDeviceName.Equals( other );
		}


		/// <summary>Inequality operator.</summary>
		/// <param name="targetDeviceName">A <see cref="TargetDeviceName"/> structure.</param>
		/// <param name="other">A <see cref="TargetDeviceName"/> structure.</param>
		/// <returns>Returns true if the structures are not equal, otherwise returns false.</returns>
		public static bool operator !=( TargetDeviceName targetDeviceName, TargetDeviceName other )
		{
			return !targetDeviceName.Equals( other );
		}


		#endregion

	}

}