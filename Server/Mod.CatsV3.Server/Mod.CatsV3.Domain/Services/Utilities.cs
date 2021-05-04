using Mod.CatsV3.Domain.Entities;
using Mod.Framework.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Mod.CatsV3.Domain.Services
{
    public static class Utilities
    {
        private static string SiteUrl = Environment.GetEnvironmentVariable("MOD.CatsV3.SPSiteUrl");
        private static string DocumentLibrary = Environment.GetEnvironmentVariable("MOD.CatsV3.DocumentLibrary");
        public static string getDocumentOpenScheme(string DocumentType = null, string TargetOpenMode = null, string FileName = null)
        {
            var TargetDocumentOpenScheme = "";
            
            DocumentType = !string.IsNullOrEmpty(FileName) ?  Path.GetExtension(FileName).Replace(".","") : DocumentType;

            TargetOpenMode = string.IsNullOrEmpty(TargetOpenMode) ? "edit" : TargetOpenMode;
            
            if (TargetOpenMode.ToLower() == "edit")
            {
                TargetOpenMode = "ofe|u|";
            }
            else
            {
                TargetOpenMode = "ofv|u|";
            }

            switch (DocumentType)
            {
                case "doc":

                    TargetDocumentOpenScheme = "ms-word";
                    return Uri.EscapeUriString(TargetDocumentOpenScheme + ":" + TargetOpenMode);

                case "docx":

                    TargetDocumentOpenScheme = "ms-word";
                    return Uri.EscapeUriString(TargetDocumentOpenScheme + ":" + TargetOpenMode);

                case "ppt":

                    TargetDocumentOpenScheme = "ms-powerpoint";
                    return Uri.EscapeUriString(TargetDocumentOpenScheme + ":" + TargetOpenMode);

                case "pptx":

                    TargetDocumentOpenScheme = "ms-powerpoint";
                    return Uri.EscapeUriString(TargetDocumentOpenScheme + ":" + TargetOpenMode);

                case "xls":

                    TargetDocumentOpenScheme = "ms-excel";
                    return Uri.EscapeUriString(TargetDocumentOpenScheme + ":" + TargetOpenMode);

                case "xlsx":

                    TargetDocumentOpenScheme = "ms-excel";
                    return Uri.EscapeUriString(TargetDocumentOpenScheme + ":" + TargetOpenMode);

                case "vsd":

                    TargetDocumentOpenScheme = "ms-visio";
                    return Uri.EscapeUriString(TargetDocumentOpenScheme + ":" + TargetOpenMode);

                case "mdb":

                    TargetDocumentOpenScheme = "ms-access";
                    return Uri.EscapeUriString(TargetDocumentOpenScheme + ":" + TargetOpenMode);

                case "mpp":

                    TargetDocumentOpenScheme = "ms-project";
                    return Uri.EscapeUriString(TargetDocumentOpenScheme + ":" + TargetOpenMode);

                case "pub":

                    TargetDocumentOpenScheme = "ms-publisher";
                    return Uri.EscapeUriString(TargetDocumentOpenScheme + ":" + TargetOpenMode);

                default:
                    return "";


            }
        }
        public static string getIcon(
            bool IsDeleted = false, 
            bool Rejected = false, 
            string LetterStatus = null, 
            string ReviewStatus = null,
            bool IsPendingLeadOffice = false,
            bool IsSaved = false)
        {

            if (IsDeleted)
            {
                return "delete";
            }
            else if (LetterStatus == "Closed")
            {
                return "lock";
            }
            else if (Rejected == true)
            {
                return "assignment_returned";
            }
            else if (IsSaved == true)
            {
                return "save";
            }
            else if (IsPendingLeadOffice == true)
            {
                return "arrow_forward";
            }
            else if (LetterStatus == "Open" && ReviewStatus == "In Progress")
            {
                return "lock_open";
            }
            else
                return "lock_open";
        }

        public static string getDocumentCount(Folder Folder)
        {
            if (Folder != null)
            {
                if (Folder.Documents != null)
                    return Folder.Documents.Count.ToString() + (Folder.Documents.Count > 1 ? " Documents" : " Document");
                else
                    return "0 Document";
            }
            else
            {
                return "0 Document";
            }
        }

        public static int getDocumentFinalCount(Folder Folder)
        {
            if (Folder != null)
            {
                if (Folder.Documents != null)
                    return Folder.Documents.Where(doc => doc.Type == "Final Document").Count();
                else
                    return 0;
            }
            else
            {
                return 0;
            }
        }

        public static int getDocumentReferenceCount(Folder Folder)
        {
            if (Folder != null)
            {
                if (Folder.Documents != null)
                    return Folder.Documents.Where(doc => doc.Type == "Reference Document").Count();
                else
                    return 0;
            }
            else
            {
                return 0;
            }
        }

        public static int getDocumentReviewCount(Folder Folder)
        {
            if (Folder != null)
            {
                if (Folder.Documents != null)
                    return Folder.Documents.Where(doc => doc.Type == "Review Document").Count();
                else
                    return 0;
            }
            else
            {
                return 0;
            }
        }
        public static string getDocuments(Folder Folder, string CATSID, string documentType)
        {
            if (Folder != null)
            {

                string res = string.Join("</br>", Folder.Documents.Where(doc => doc.Type == documentType)
                    .Select(x =>
                    {
                        string filename = x.Name;//x.Name.Split("--")[x.Name.Split("--").Length - 1];
                        filename = Uri.UnescapeDataString(filename);
                        string foldeName = string.IsNullOrEmpty(Folder.Name) ? CATSID : Folder.Name.Trim();
                        string path = Utilities.getDocumentOpenScheme(null, null, filename) + (Path.Combine(SiteUrl, DocumentLibrary, foldeName, x.Name).Replace("\\","/"));
                        //path = Uri.EscapeUriString(path);
                        return "<a title=\"" + filename + "\" href=\"" + path + "\" target=\"_blank\">" + filename + "</a>";
                    }).ToArray());
                return String.Format("{0}" + res + "{1}", "<div style=\"max-height: 150px;overflow-y: auto; overflow-x: hidden;text-overflow: ellipsis;white-space: nowrap \">", "</div>");
            }
            else
            {
                return null;
            }
        }
        public static string getAssignedUsers(string userType)
        {
            if (!string.IsNullOrEmpty(userType))
            {

                string res = string.Join("</br>", string.IsNullOrEmpty(userType) ? Array.Empty<string>() : userType.Split(";")
                    .Select(user =>
                    {
                        return "<span title='" + user + "' >" + user + "</span>";
                    }));
                return String.Format("{0}" + res + "{1}", "<div style='max-height: 150px;overflow-y: auto; overflow-x: hidden;text-overflow: ellipsis;white-space: nowrap '>", "</div>");
            }
            else
            {
                return "";
            }
        }
        public static string getCompletedRounds(string CompletedRounds)
        {
            if (!string.IsNullOrEmpty(CompletedRounds))
            {
                string res = string.Join("</br>", string.IsNullOrEmpty(CompletedRounds) ? Array.Empty<string>() : CompletedRounds.Split(",")
                    .Select(user =>
                    {
                        return "<span title='" + user + "'>" + user + "</span>";
                    }));
                return String.Format("{0}" + res + "{1}", "<div style='max-height: 150px;overflow-y: auto; overflow-x: hidden;text-overflow: ellipsis;white-space: nowrap '>", "</div>");
            }
            else
            {
                return "";
            }
        }

        public static Expression<Func<TSource, bool>>[] CreateBinaryExpressionContains22<TSource>(string filter, string[] properties)
        {
            List<Expression<Func<TSource, bool>>> listExpressions = new List<Expression<Func<TSource, bool>>>();

            properties.ToList().ForEach(
                property =>
                {
                    // Create the parameter (typically the 'x' in your expression (x => 'x')
                    ParameterExpression param = Expression.Parameter(typeof(TSource), property);

                    // Store the result of a calculated Expression
                    Expression exp = null;

                    // The member you want to evaluate (x => x.FirstName)
                    MemberExpression member = Expression.Property(param, property);

                    // The method you want to use to create an 'IN' statement
                    MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });

                    // The value you want to evaluate
                    ConstantExpression constant = Expression.Constant(filter);
                    BinaryExpression result = Expression.Or(member, constant, method);

                    // Actually apply the expression together (x => x.FirstName.Contains('value'))
                    //MethodCallExpression result = Expression.Call(member, method, constant);
                    var InnerLambda = Expression.Call(member, typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), constant);

                    // The Result
                    exp = result;

                    // Creates the expression against the type
                    Expression<Func<TSource, bool>> deleg = Expression.Lambda<Func<TSource, bool>>(exp, param);
                    listExpressions.Add(deleg);
                }
                );


            return listExpressions.ToArray();

        }
        public static Expression<Func<TSource, bool>>[] CreateBinaryExpressionContains<TSource>(string filter, string[] properties)
        {
            List<Expression<Func<TSource, bool>>> listExpressions = new List<Expression<Func<TSource, bool>>>();

            properties.ToList().ForEach(
                property => 
                    {
                        // Create the parameter (typically the 'x' in your expression (x => 'x')
                        ParameterExpression param = Expression.Parameter(typeof(TSource), property);

                        // Store the result of a calculated Expression
                        Expression exp = null;

                        // The member you want to evaluate (x => x.FirstName)
                        MemberExpression member = Expression.Property(param, property);

                        // The method you want to use to create an 'IN' statement
                        MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });


                        // The value you want to evaluate
                        ConstantExpression constant = Expression.Constant(filter);
                        //BinaryExpression result = Expression.Equal(member, constant);
                        //BinaryExpression result = Expression.Or(member, constant, method);

                        // Actually apply the expression together (x => x.FirstName.Contains('value'))
                        MethodCallExpression result = Expression.Call(member, method, constant);

                        // The Result
                        exp = result;

                        // Creates the expression against the type
                        Expression<Func<TSource, bool>> deleg = Expression.Lambda<Func<TSource, bool>>(exp, param);
                        listExpressions.Add(deleg);
                    }
                );


            return listExpressions.ToArray();

        }

        public static string[] getClassProperties<TSource>()
        {
            List<string> ListProps = new List<string>();

            // Get the type of FieldsClass.
            Type fieldsType = typeof(TSource);
            PropertyInfo[] myPropertyInfo;
            // Get the properties of 'Type' class object.
            myPropertyInfo = fieldsType.GetProperties();
            Console.WriteLine("Properties of System.Type are:");
            for (int i = 0; i < myPropertyInfo.Length; i++)
            {
                string propertyName = myPropertyInfo[i].PropertyType.Name;
                if (!myPropertyInfo[i].IsSpecialName && (propertyName == "String"))
                {
                    Console.WriteLine(myPropertyInfo[i].Name);
                    ListProps.Add(myPropertyInfo[i].Name);
                }

            }

            return ListProps.ToArray();
        }
        public static Expression<Func<T, bool>> setPropertySelectors<T>(string filter, string[] props)
        {  
            var d = Utilities.ToExpression<T>("And", string.Join(".", props), "Like", filter.Trim());
            return d;
        }

        public static Expression<Func<T, bool>>[] setPropertySelectorsForOffices<T>(string[] filter)
        {
            string[] props = new string[] {"LeadOfficeName", "CopiedOfficeName"};
            List<Expression<Func<T, bool>>> ListExpressions = new List<Expression<Func<T, bool>>>();

            filter.ToList().ForEach(x => {
                var d = Utilities.ToExpression<T>("And", string.Join(".", props), "Like", x.Trim());
                ListExpressions.Add(d);
            });
            
            return ListExpressions.ToArray();
        }

        public static Expression<Func<T, bool>> ToExpression<T>(string andOrOperator, string propName, string opr, string value, Expression<Func<T, bool>> expr = null)
        {
            Expression<Func<T, bool>> func = null;
            try
            {
                ParameterExpression paramExpr = Expression.Parameter(typeof(T));
                var arrProp = propName.Split('.').ToList();
                Expression binExpr = null;
                string partName = string.Empty;
                arrProp.ForEach(x =>
                {
                    Expression tempExpr = null;

                    var member = NestedExprProp(paramExpr, x);
                    var type = member.Type.Name == "Nullable`1" ? Nullable.GetUnderlyingType(member.Type) : member.Type;
                    tempExpr = ApplyFilter(opr, member, Expression.Convert(ToExprConstant(type, value), member.Type));

                    if (binExpr != null)
                        binExpr = Expression.Or(binExpr, tempExpr);
                    else
                        binExpr = tempExpr;
                });
                
                Expression<Func<T, bool>> innerExpr = Expression.Lambda<Func<T, bool>>(binExpr, paramExpr);

                //if (expr != null)
                //    innerExpr = (string.IsNullOrEmpty(andOrOperator) || andOrOperator == "And" || andOrOperator == "AND" || andOrOperator == "&&") ?
                //         AndAlso(innerExpr, expr) :
                //         Expression.Lambda<Func<T, bool>>(
                //        Expression.Or(innerExpr, expr));
                func = innerExpr;
            }
            catch(Exception ex) {
                Console.Out.WriteLine(ex.Message);
            }
            return func;
        }

        public static Expression<TDelegate> AndAlso<TDelegate>(this Expression<TDelegate> left, Expression<TDelegate> right)
        {
            // NOTICE: Combining BODIES:
            return Expression.Lambda<TDelegate>(Expression.AndAlso(left.Body, right.Body), left.Parameters);
        }

        private static MemberExpression NestedExprProp(Expression expr, string propName)
        {
            string[] arrProp = propName.Split('.');
            int arrPropCount = arrProp.Length;
            return (arrPropCount > 1) ? Expression.Property(NestedExprProp(expr, arrProp.Take(arrPropCount - 1).Aggregate((a, i) => a + "." + i)), arrProp[arrPropCount - 1]) : Expression.Property(expr, propName);
        }

        private static Expression ToExprConstant(Type prop, string value)
        {
            if (string.IsNullOrEmpty(value))
                return Expression.Constant(value);
            object val = null;
            switch (prop.FullName)
            {
                case "System.Guid":
                    //val = value.ToGuid();
                    break;
                default:
                    val = Convert.ChangeType(value, Type.GetType(prop.FullName));
                    break;
            }
            return Expression.Constant(val);
        }

        private static Expression ApplyFilter(string opr, Expression left, Expression right)
        {
            Expression InnerLambda = null;
            switch (opr.ToUpper())
            {
                case "==":
                case "=":
                    InnerLambda = Expression.Equal(left, right);
                    break;
                case "<":
                    InnerLambda = Expression.LessThan(left, right);
                    break;
                case ">":
                    InnerLambda = Expression.GreaterThan(left, right);
                    break;
                case ">=":
                    InnerLambda = Expression.GreaterThanOrEqual(left, right);
                    break;
                case "<=":
                    InnerLambda = Expression.LessThanOrEqual(left, right);
                    break;
                case "!=":
                    InnerLambda = Expression.NotEqual(left, right);
                    break;
                case "&&":
                    InnerLambda = Expression.And(left, right);
                    break;
                case "||":
                    InnerLambda = Expression.Or(left, right);
                    break;
                case "LIKE":
                    InnerLambda = Expression.Call(left, typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), right);
                    break;
                case "NOTLIKE":
                    InnerLambda = Expression.Not(Expression.Call(left, typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), right));
                    break;
            }
            return InnerLambda;
        }

        public static Expression<Func<T, bool>> CreateEqualityExpressionForFilter<T>(object filter, string EntityFields)
        {
            var lambdaParam = Expression.Parameter(typeof(T));

            var leftExpression = Expression.PropertyOrField(lambdaParam, EntityFields);

            var filterValue = Convert.ChangeType(filter, typeof(object));

            Expression<Func<object>> closure = () => filterValue;
            var rightExpression = Expression.Convert(closure.Body, leftExpression.Type);

            var lambdaBody = Expression.Equal(leftExpression, rightExpression);
            //Expression conversion = Expression.Convert(lambdaBody, typeof(object));

            return Expression.Lambda<Func<T, bool>>(lambdaBody, lambdaParam);
        }

        public static Expression<Func<T, object>> ToExpression<T>(T entity, string property)
        {
            var constant = Expression.Constant(entity);
            var memberExpression = Expression.Property(constant, property);
            Expression convertExpr = Expression.Convert(memberExpression, typeof(object));
            var expression = Expression.Lambda(convertExpr);

            return (Expression<Func<T, object>>)expression;
        }

        //public static Expression<Func<T, object>> PropExpr<T>(string PropName)
        //{
        //    ParameterExpression paramExpr = Expression.Parameter(typeof(T));
        //    var tempExpr = Extentions.NestedExprProp(paramExpr, PropName);
        //    return Expression.Lambda<Func<T, object>>(Expression.Convert(Expression.Lambda(tempExpr, paramExpr).Body, typeof(object)), paramExpr);

        //}
        //public static IQueryOver<T, T> OrderBy<T>(this IQueryOver<T, T> Collection, string sidx, string sord)
        //{
        //    return sord == "asc" ? Collection.OrderBy(NHibernate.Criterion.Projections.Property(sidx)).Asc : Collection.OrderBy(NHibernate.Criterion.Projections.Property(sidx)).Desc;
        //}

        //public static Expression<Func<T, TResult>> And<T, TResult>(this Expression<Func<T, TResult>> expr1, Expression<Func<T, TResult>> expr2)
        //{
        //    var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
        //    return Expression.Lambda<Func<T, TResult>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        //}

        //public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        //{
        //    var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
        //    return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        //}
    }
}
