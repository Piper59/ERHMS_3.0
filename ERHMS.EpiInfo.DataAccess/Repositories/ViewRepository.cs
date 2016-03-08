﻿using Epi;
using ERHMS.EpiInfo.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace ERHMS.EpiInfo.DataAccess
{
    public class ViewRepository<TEntity> : RepositoryBase<TEntity> where TEntity : ViewEntity, new()
    {
        protected DataTable BaseSchema { get; private set; }
        protected DataSet PageSchemas { get; private set; }
        public View View { get; private set; }

        public ViewRepository(IDataDriver driver, string viewName)
            : base(driver)
        {
            View = Project.Views[viewName];
            if (!Project.CollectedData.TableExists(View.TableName))
            {
                Project.CollectedData.CreateDataTableForView(View, 1);
            }
            BaseSchema = GetSchema(View.TableName);
            PageSchemas = new DataSet();
            foreach (Page page in View.Pages)
            {
                PageSchemas.Tables.Add(GetSchema(page.TableName));
            }
        }

        public virtual TEntity Create()
        {
            TEntity entity = new TEntity();
            foreach (DataColumn column in BaseSchema.Columns)
            {
                entity.SetProperty(column.ColumnName, null);
            }
            foreach (Page page in View.Pages)
            {
                foreach (DataColumn column in PageSchemas.Tables[page.TableName].Columns)
                {
                    entity.SetProperty(column.ColumnName, null);
                }
            }
            return entity;
        }

        public override IEnumerable<TEntity> Select()
        {
            ICollection<TEntity> entities;
            {
                string sql = string.Format("SELECT * FROM {0}", Driver.Escape(View.TableName));
                entities = Mapper.GetEntities(Driver.ExecuteQuery(sql)).ToList();
            }
            foreach (Page page in View.Pages)
            {
                string sql = string.Format("SELECT * FROM {0}", Driver.Escape(page.TableName));
                DataTable data = Driver.ExecuteQuery(sql);
                Mapper.SetEntities(data, data.Columns[ColumnNames.GLOBAL_RECORD_ID], entities, StringComparison.OrdinalIgnoreCase);
            }
            return entities;
        }

        protected string GetJoinSql()
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(Driver.Escape(View.TableName));
            foreach (Page page in View.Pages)
            {
                sql.Insert(0, "(");
                sql.Append(string.Format(
                    ") INNER JOIN {1} ON {0}.{2} = {1}.{2}",
                    Driver.Escape(View.TableName),
                    Driver.Escape(page.TableName),
                    ColumnNames.GLOBAL_RECORD_ID));
            }
            return sql.ToString();
        }

        public override IEnumerable<TEntity> Select(DataPredicate predicate)
        {
            ICollection<TEntity> entities;
            string sqlFormat = string.Format("SELECT {{0}}.* FROM {0} WHERE {{1}}", GetJoinSql());
            {
                string sql = string.Format(sqlFormat, Driver.Escape(View.TableName), predicate.Sql);
                entities = Mapper.GetEntities(Driver.ExecuteQuery(sql, predicate.Parameters)).ToList();
            }
            foreach (Page page in View.Pages)
            {
                string sql = string.Format(sqlFormat, Driver.Escape(page.TableName), predicate.Sql);
                DataTable data = Driver.ExecuteQuery(sql, predicate.Parameters);
                Mapper.SetEntities(data, data.Columns[ColumnNames.GLOBAL_RECORD_ID], entities, StringComparison.OrdinalIgnoreCase);
            }
            return entities;
        }

        public virtual TEntity SelectById(string globalRecordId)
        {
            DataParameter parameter;
            string sql = GetEqualitySql(BaseSchema.Columns[ColumnNames.GLOBAL_RECORD_ID], globalRecordId, out parameter);
            DataPredicate predicate = new DataPredicate(sql, parameter);
            return Select(predicate).SingleOrDefault();
        }

        public virtual void Insert(TEntity entity, IIdentity user = null)
        {
            if (entity.GlobalRecordId == null)
            {
                entity.GlobalRecordId = Guid.NewGuid().ToString();
            }
            if (!entity.RecordStatus.HasValue)
            {
                entity.SetDeleted(false);
            }
            entity.SetAuditProperties(true, true, user);
            using (DataTransaction transaction = Driver.BeginTransaction())
            {
                Insert(entity, BaseSchema, transaction);
                foreach (Page page in View.Pages)
                {
                    Insert(entity, PageSchemas.Tables[page.TableName], transaction);
                }
                transaction.Commit();
            }
            entity.UniqueKey = SelectById(entity.GlobalRecordId).UniqueKey;
            entity.IsNew = false;
        }

        public virtual void Update(TEntity entity, IIdentity user = null)
        {
            entity.SetAuditProperties(false, true, user);
            using (DataTransaction transaction = Driver.BeginTransaction())
            {
                Update(entity, BaseSchema, transaction);
                foreach (Page page in View.Pages)
                {
                    Update(entity, PageSchemas.Tables[page.TableName], transaction);
                }
                transaction.Commit();
            }
        }

        public virtual void Save(TEntity entity, IIdentity user = null)
        {
            if (entity.IsNew)
            {
                Insert(entity, user);
            }
            else
            {
                Update(entity, user);
            }
        }

        public virtual void Delete(TEntity entity, IIdentity user = null)
        {
            entity.SetDeleted(true);
            Save(entity, user);
        }

        public virtual void Undelete(TEntity entity, IIdentity user = null)
        {
            entity.SetDeleted(false);
            Save(entity, user);
        }
    }
}
