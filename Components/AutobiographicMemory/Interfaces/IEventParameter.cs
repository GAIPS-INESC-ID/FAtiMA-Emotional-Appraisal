﻿using System;

namespace AutobiographicMemory.Interfaces
{
	public interface IEventParameter : ICloneable
	{
		string ParameterName
		{
			get;
		}

		object Value
		{
			get;
		}
	}
}