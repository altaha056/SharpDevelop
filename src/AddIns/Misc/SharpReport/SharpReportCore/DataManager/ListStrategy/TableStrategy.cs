//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a tool.
//     Runtime Version: 1.1.4322.2032
//
//     Changes to this file may cause incorrect behavior and will be lost if 
//     the code is regenerated.
// </autogenerated>
//------------------------------------------------------------------------------
using System;
using System.Text;
using System.Collections;
using System.Data;
using System.ComponentModel;
using System.Windows.Forms;
using SharpReportCore;

namespace SharpReportCore {
	
	/// <summary>
	/// This class handles DataTables
	/// </summary>
	/// <remarks>
	/// 	created by - Forstmeier Peter
	/// 	created on - 23.10.2005 15:12:06
	/// </remarks>
	public class TableStrategy : BaseListStrategy {
		
		DataTable table;
		DataView view = new DataView();
		DataRowView row;
		
		
		public TableStrategy(DataTable table,ReportSettings reportSettings):base(reportSettings) {
			this.table = table;
//			view.ListChanged += new ListChangedEventHandler (OnListChange);
		}
		
//		private void OnListChange (object sender,ListChangedEventArgs e) {
//			System.Console.WriteLine("called from view");
//			MessageBox.Show ("On List Change");
//		}

		private string a_BuildSort(ColumnCollection sortCollection){
			System.Console.WriteLine("BuildSort");
			StringBuilder sb = new StringBuilder();	
		
			for (int i = 0;i < sortCollection.Count ;i++ ) {
				SortColumn sc = (SortColumn)sortCollection[i];
				sb.Append(sc.ColumnName);
				if (sc.SortDirection == ListSortDirection.Ascending) {
					sb.Append (" ASC");
				} else {
					sb.Append(" DESC");
				}
				sb.Append (",");
			}
			if (sb.ToString().EndsWith (",")) {
				sb.Remove(sb.Length -1,1);
			}
			System.Console.WriteLine("\tsort by {0}",sb.ToString());
			return sb.ToString();
		}
		#region Building the Index list
		
		// if we have no sorting, we build the indexlist as well, so we don't need to
		//check each time we reasd data if we have to go directly or by IndexList
		private  ArrayList BuildSortIndex(ColumnCollection col) {
			
			ArrayList sortValues = new ArrayList(this.view.Count);
			try {
				for (int rowIndex = 0; rowIndex < this.view.Count; rowIndex++){
					DataRowView rowItem = this.view[rowIndex];
					object[] values = new object[col.Count];
					for (int criteriaIndex = 0; criteriaIndex < col.Count; criteriaIndex++){
						AbstractColumn c = (AbstractColumn)col[criteriaIndex];
						object value = rowItem[c.ColumnName];

						if (value != null && value != DBNull.Value){
							if (!(value is IComparable)){
								throw new InvalidOperationException("ReportDataSource:BuildSortArray - > This type doesn't support IComparable." + value.ToString());
							}
							
							values[criteriaIndex] = value;
						}   else {
							values[criteriaIndex] = DBNull.Value;
						}
					}
					sortValues.Add(new SortComparer(col, rowIndex, values));
				}
			} catch (Exception) {
				
			}
			sortValues.Sort();
			return sortValues;
		}

		private  ArrayList BuildPlainIndex(ColumnCollection col) {
			ArrayList sortValues = new ArrayList(this.view.Count);
			try {
				for (int rowIndex = 0; rowIndex < this.view.Count; rowIndex++){
					object[] values = new object[1];
					
					// We insert only the RowNr as a dummy value
					values[0] = rowIndex;
					sortValues.Add(new BaseComparer(col, rowIndex, values));
				}
			} catch (Exception e) {
				throw e;
			}
			return sortValues;;
		}
		
		
		private  ArrayList BuildGroupIndex(ColumnCollection col) {
			ArrayList groupValues = new ArrayList(this.view.Count);
			System.Console.WriteLine("\tBuildGroupIndex");
			try {
				for (int rowIndex = 0; rowIndex < this.view.Count; rowIndex++){
					DataRowView rowItem = this.view[rowIndex];
					object[] values = new object[col.Count];
					for (int criteriaIndex = 0; criteriaIndex < col.Count; criteriaIndex++){
						AbstractColumn c = (AbstractColumn)col[criteriaIndex];
						object value = rowItem[c.ColumnName];

						if (value != null && value != DBNull.Value){
							if (!(value is IComparable)){
								throw new InvalidOperationException("ReportDataSource:BuildSortArray - > This type doesn't support IComparable." + value.ToString());
							}
							
							values[criteriaIndex] = value;
						}   else {
							values[criteriaIndex] = DBNull.Value;
						}
					}
					groupValues.Add(new GroupComparer(col, rowIndex, values));
				}
			} catch (Exception) {
				
			}
			groupValues.Sort();
		
			return groupValues;
		}

	
		
		#endregion
		
		#region Grouping
		
		private void WriteToIndexFile (ArrayList destination,int index,GroupComparer comparer) {
			destination.Add(comparer);
		}
		
		private void BuildGroupSeperator (ArrayList destination,BaseComparer newGroup,int groupLevel) {
			
			GroupSeperator seperator = new GroupSeperator (newGroup.ColumnCollection,
			                                               newGroup.ListIndex,
			                                               newGroup.ObjectArray,
			                                               groupLevel);
			
			
//		System.Console.WriteLine("\t Group change {0} level {1}",seperator.ObjectArray[0].ToString(),
//			                         seperator.GroupLevel);
//			System.Console.WriteLine("write group seperator");
			destination.Add(seperator);
		}
		
		private ArrayList InsertGroupRows (ArrayList sourceList) {
			ArrayList destList = new ArrayList();
			
			int level = 0;
			
//			// only for testing
//			ColumnCollection grBy = base.ReportSettings.GroupColumnsCollection;
//			string columnName = grBy[level].ColumnName;
////			System.Console.WriteLine("");
//			System.Console.WriteLine("InsertGroupRows Grouping for  {0}",columnName);
	
			GroupComparer compareComparer = null;
			
			for (int i = 0;i < sourceList.Count ;i++ ) {
				GroupComparer currentComparer = (GroupComparer)sourceList[i];
				
				if (compareComparer != null) {
					string str1,str2;
					str1 = currentComparer.ObjectArray[0].ToString();
					str2 = compareComparer.ObjectArray[0].ToString();
					int compareVal = str1.CompareTo(str2);
					
					if (compareVal != 0) {
						this.BuildGroupSeperator (destList,currentComparer,level);
					}
				}
				else {
//					System.Console.WriteLine("\t\t Start of List {0}",currentComparer.ObjectArray[0].ToString());
					this.BuildGroupSeperator (destList,currentComparer,level);
				}
				this.WriteToIndexFile (destList,i,currentComparer);
				compareComparer = (GroupComparer)sourceList[i];
			}
			return destList;
		}
		
		private void BuildGroup(){
			try {
				ArrayList groupedArray = new ArrayList();
				
				if (base.ReportSettings.GroupColumnsCollection != null) {
					if (base.ReportSettings.GroupColumnsCollection.Count > 0) {
						groupedArray =  this.BuildGroupIndex (base.ReportSettings.GroupColumnsCollection);
						
					} else {
						groupedArray =  BuildPlainIndex (base.ReportSettings.GroupColumnsCollection);
					}
				}
				
				base.IndexList.Clear();
				base.IndexList.AddRange (InsertGroupRows(groupedArray));
				
				if (base.IndexList == null){
					throw new NotSupportedException("Sortieren f�r die Liste nicht unterst�tzt.");
				}
			} catch (Exception) {
				throw;
			}
		}
		
		
		protected override void Group() {
			if (base.ReportSettings.GroupColumnsCollection.Count == 0) {
				return;
			}
			
			try {
				this.BuildGroup();
				base.Group();
			
				if (this.IsGrouped == false) {
					throw new SharpReportException("TableStratregy:Group Error in grouping");
				}
			} catch (Exception) {
				base.IsGrouped = false;
				base.IsSorted = false;
				throw;
			}
		}

		#endregion
		
		
		#region IDataViewStrategy interface implementation
		
		public override void Bind() {
			base.Bind();
			view = this.table.DefaultView;
			
			if ((base.ReportSettings.GroupColumnsCollection != null) && (base.ReportSettings.GroupColumnsCollection.Count > 0)) {
				this.Group ();
				Reset();
				base.FireResetList();
				return;
			}
			
			if (base.ReportSettings.SortColumnCollection != null) {
				this.Sort ();
			}
			Reset();
			base.FireResetList();
		}
	
		
		public override  void Sort () {
			base.Sort();
			ArrayList sortedArray = new ArrayList();
			try {
				if ((base.ReportSettings.SortColumnCollection != null)) {
					if (base.ReportSettings.SortColumnCollection.Count > 0) {
						SortColumn sc = (SortColumn)base.ReportSettings.SortColumnCollection[0];
	
						sortedArray =  this.BuildSortIndex (base.ReportSettings.SortColumnCollection);
						base.IsSorted = true;
					} else {
						sortedArray =  BuildPlainIndex (base.ReportSettings.SortColumnCollection);
						base.IsSorted = false;
					}
				}
				
				base.IndexList.Clear();
				base.IndexList.AddRange (sortedArray);
				
//				base.CheckSortArray (sortedArray,"TableStrategy - CheckSortArray");
			} catch (Exception) {
				throw;
			}
			
			if (base.IndexList == null){
				throw new NotSupportedException("Sortieren f�r die Liste nicht unterst�tzt.");
			}
		}
		
		public override void Fill (IItemRenderer item) {
			try {
				base.Fill(item);
				BaseDataItem baseDataItem = item as BaseDataItem;
				if (baseDataItem != null) {
					baseDataItem.DbValue = row[baseDataItem.ColumnName].ToString();
				}
			} catch (Exception ) {
			}
		}
			
		
		public override void Reset() {
			this.CurrentRow = 0;
			this.view.Sort = "";
			this.view.RowFilter = "";
			base.Reset();
		}
		
		public override ColumnCollection AvailableFields {
			get {
				ColumnCollection c = base.AvailableFields;
				DataTable tbl = view.Table;
				for (int i = 0;i < tbl.Columns.Count ;i ++ ) {
					DataColumn col = tbl.Columns[i];
					c.Add (new AbstractColumn(col.ColumnName,col.DataType));
					}
				return c;
			}
		}
		
		
		public override int Count {
			get {
				return this.table.Rows.Count;
			}
		}
		
		public override int CurrentRow {
			get{
				return base.IndexList.CurrentPosition;
			}
			set {
				base.CurrentRow = value;
	
				if (base.IndexList.Count > 0) {
					BaseComparer bc = (BaseComparer)base.IndexList[value];
					
					GroupSeperator sep = bc as GroupSeperator;
					if (sep != null) {
						base.FireGroupChange(this,sep);
					}
					row = this.view[((BaseComparer)base.IndexList[value]).ListIndex];
				}
			}
		}
		
		

		
		public override bool IsSorted {
			get {
				return (this.view.Sort.Length > 0);
			}
		}
		
		#endregion
		
	}
}
