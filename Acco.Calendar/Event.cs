﻿using Acco.Calendar.Location;
using Acco.Calendar.Person;
using DDay.iCal;
using RecPatt = DDay.iCal.RecurrencePattern;
//
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

//

namespace Acco.Calendar.Event
{
    public interface IRecurrence
    {
        void Parse<T>(T rules);

        string Get();
    }

    public class GenericRecurrence : IRecurrence
    {
        public RecPatt Pattern { get; set; }

        public virtual void Parse<T>(T rules)
        {
            throw new NotImplementedException();
        }

        public virtual string Get()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            var s = "";
            if (Pattern != null)
            {
                s = Pattern.ToString();
            }
            return s;
        }
    }

    [Serializable]
    public class RecurrenceParseException : Exception
    {
        public RecurrenceParseException(string message, Type typeOfRule) :
            base(message)
        {
            TypeOfRule = typeOfRule;
        }

        public Type TypeOfRule { get; private set; }
    }

    public interface IEvent
    {
        [Required(ErrorMessage = "This field is required")]
        string Id { get; set; }

        GenericPerson Organizer { get; set; }

        GenericPerson Creator { get; set; }

        DateTime? Created { get; set; }

        DateTime? LastModified { get; set; }

        DateTime? Start { get; set; }

        DateTime? End { get; set; }

        [Required(ErrorMessage = "This field is required")]
        string Summary { get; set; }

        [Required(ErrorMessage = "This field is required")]
        string Description { get; set; }

        [Required(ErrorMessage = "This field is required")]
        GenericLocation Location { get; set; }

        GenericRecurrence Recurrence { get; set; }

        List<GenericAttendee> Attendees { get; set; }

        EventAction EventAction { get; set; }
    }

    public enum EventAction : sbyte
    {
        Add = 0,
        Update,
        Remove,
        Duplicate
    }

    public class GenericEvent : IEvent
    {
        public GenericEvent(string id)
        {
            Id = id;
            Summary = "No Summary";
            Description = "No Description";
            Location = new GenericLocation { Name = "No Location" };
        }

        public GenericEvent(string id, string summary, string description)
        {
            Id = id;
            Summary = summary;
            Description = description;
            Location = new GenericLocation { Name = "No Location" };
        }

        public GenericEvent(string id, string summary, string description, ILocation location)
        {
            Id = id;
            Summary = summary;
            Description = description;
            Location = location as GenericLocation;
        }

        public string Id { get; set; }

        public GenericPerson Organizer { get; set; }

        public GenericPerson Creator { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? LastModified { get; set; }

        public DateTime? Start { get; set; }

        public DateTime? End { get; set; }

        public string Summary { get; set; }

        public string Description { get; set; }

        public GenericLocation Location { get; set; }

        public GenericRecurrence Recurrence { get; set; }

        public List<GenericAttendee> Attendees { get; set; }

        public override string ToString()
        {
            var eventString = "[";
            eventString += "Id: " + Id;
            eventString += "\n";
            eventString += "Summary: " + Summary;
            eventString += "\n";
            eventString += "Location: " + Location.Name;
            eventString += "]";
            return eventString;
        }

        public EventAction EventAction { get; set; }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            var p = obj as GenericEvent;
            if ((object)p == null)
            {
                return false;
            }

            // Event comparison - Id, Dates, Location, Recurrence, Attendees
            return (this == p);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(GenericEvent e1, GenericEvent e2)
        {
            if((object)e1 != null &&
                (object)e2 != null)
            { 
                return  (e1.Id == e2.Id) &&
                        (e1.Start == e2.Start) && 
                        (e1.End == e2.End) &&
                        (e1.Location == e2.Location) &&
                        (e1.Recurrence != null && e2.Recurrence != null) &&
                        (e1.Recurrence.Pattern == e2.Recurrence.Pattern) &&
                        (e1.Attendees.Count == e2.Attendees.Count /* stupid comparison, but enough to trigger the update */);
            }
            else
            {
                return (object)e1 == (object)e2;
            }
        }

        public static bool operator !=(GenericEvent e1, GenericEvent e2)
        {
            return !(e1 == e2);
        }
    }
}