﻿using Acco.Calendar.Database;
using Acco.Calendar.Event;
using Acco.Calendar.Person;
using MongoDB.Driver.Builders;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Acco.Calendar
{
    public interface IDbActions<in T> where T: IEvent
    {
        void Save(T item);
        void Delete(T item);
        bool IsAlreadySynced(T item);
    }

    public class DbCollection<T> : Collection<T>, IDbActions<T> where T: IEvent
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool AddDuplicates { get; set; }

        public void Save(T item)
        {
            var r = Storage.Instance.Appointments.Save(item);
            if (!r.Ok)
            {
                Log.Error(String.Format("[{0}] was not added", item.Id));
            }
            else
            {
                Log.Info(String.Format("[{0}] was added", item.Id));
            }
        }

        public void Delete(T item)
        {
            var query = Query<T>.EQ(e => e.Id, item.Id);
            var r = Storage.Instance.Appointments.Remove(query);
            if (!r.Ok)
            {
                Log.Warn(String.Format("[{0}] was not removed", item.Id));
            }
            else
            {
                Log.Info(String.Format("[{0}] was removed", item.Id));
            }
        }

        public bool IsAlreadySynced(T item)
        {
            bool isPresent;
            // first: check if the item has already been added to the shared database
            var query = Query<T>.EQ(x => x.Id, item.Id);
            if (Storage.Instance.Appointments.FindOneAs<T>(query) == null)
            {
                Log.Debug(String.Format("[{0}] was not found", item.Id));
                isPresent = false;
            }
            else 
            { 
                Log.Warn(String.Format("[{0}] already on database", item.Id));
                isPresent = true;
            }
            return isPresent;
        }

        protected override void InsertItem(int index, T item)
        {
            if(IsAlreadySynced(item) == false)
            {
                Save(item); 
                item.EventAction = EventAction.Add;
            }
            else
            {
                item.EventAction = EventAction.Duplicate;
            }
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            Items[index].EventAction = EventAction.Remove;
            Delete(Items[index]);
            base.RemoveItem(index);
        }
    }

    public interface ICalendar
    {
        string Id { get; set; }

        string Name { get; set; }

        GenericPerson Creator { get; set; }

        DbCollection<GenericEvent> Events { get; set; }
    }

    public class GenericCalendar : ICalendar
    {
        [Required(ErrorMessage = "This field is required")]
        public string Id { get; set; }

        public string Name { get; set; }

        public GenericPerson Creator { get; set; }

        public DbCollection<GenericEvent> Events { get; set; }
    }
}