﻿
using Acco.Calendar.Event;
using Acco.Calendar.Person;
using System.Collections.Generic;

namespace Acco.Calendar
{
    public interface ICalendar
    {
        string Id { get; set; }
        string Name { get; set; }
        GenericPerson Creator { get; set; }
        List<GenericEvent> Events { get; set; }
    }
    public class GenericCalendar : ICalendar
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public GenericPerson Creator { get; set; }
        public List<GenericEvent> Events { get; set; }
    }
}