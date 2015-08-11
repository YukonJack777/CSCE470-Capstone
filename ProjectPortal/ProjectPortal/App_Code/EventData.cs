using System;

namespace EventScheduler
{
	public class EventData
	{
		public string Subject { get; set; }
		public string Level { get; set; }
		public string Track { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public string Annotations { get; set; }
	}
}