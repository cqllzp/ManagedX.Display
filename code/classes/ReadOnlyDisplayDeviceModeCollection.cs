﻿using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace ManagedX.Display
{

	/// <summary>A read-only collection of <see cref="DisplayDeviceMode"/> structures.</summary>
	[System.Diagnostics.DebuggerStepThrough]
	public sealed class ReadOnlyDisplayDeviceModeCollection : ReadOnlyCollection<DisplayDeviceMode>
	{
		
		/// <summary>Instantiates and initializes a new <see cref="ReadOnlyDisplayDeviceModeCollection"/>.</summary>
		/// <param name="list">The list to be wrapped.</param>
		internal ReadOnlyDisplayDeviceModeCollection( IList<DisplayDeviceMode> list )
			: base( list )
		{
		}

	}

}