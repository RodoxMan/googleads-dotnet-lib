// Copyright 2013, Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// Author: api.anash@gmail.com (Anash P. Oommen)

using Google.Api.Ads.Dfp.v201308;

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Google.Api.Ads.Dfp.Util.v201308 {
  /// <summary>
  /// A utility class for handling PQL objects.
  /// </summary>
  public class PqlUtilities {
    /// <summary>
    /// Gets the underlying value of the Value object.
    /// </summary>
    /// <value>The Value object to get the value from.</value>
    /// <returns>The underlying value.</returns>
    public static object GetValue(Value value) {
      PropertyInfo propInfo = value.GetType().GetProperty("value");
      if (propInfo != null) {
        return propInfo.GetValue(value, null);
      }
      return null;
    }

    /// <summary>
    /// Gets the result set as list of string arrays.
    /// </summary>
    /// <param param name="resultSet">The result set to convert to a string array list.</param>
    /// <returns>A list of string arrays representing the result set.</returns>
    public static List<String[]> ResultSetToStringArrayList(ResultSet resultSet) {
      List<string[]> stringArrayList = new List<string[]>();
      stringArrayList.Add(GetColumnLabels(resultSet));
      if (resultSet.rows != null) {
        foreach (Row row in resultSet.rows) {
          stringArrayList.Add(GetRowStringValues(row));
        }
      }
      return stringArrayList;
    }

    /// <summary>
    /// Gets the result set as a table represenation in the form of:
    ///
    /// <pre>
    /// +-------+-------+-------+
    /// |column1|column2|column3|
    /// +-------+-------+-------+
    /// |value1 |value2 |value3 |
    /// +-------+-------+-------+
    /// |value1 |value2 |value3 |
    /// +-------+-------+-------+
    /// </pre>
    /// </summary>
    /// <param name="resultSet">The result set to display as a string</param>
    /// <returns>The string represenation of result set as a table.</returns>
    public static String ResultSetToString(ResultSet resultSet) {
      StringBuilder resultSetStringBuilder = new StringBuilder();
      List<String[]> resultSetStringArrayList = ResultSetToStringArrayList(resultSet);
      List<int> maxColumnSizes = GetMaxColumnSizes(resultSetStringArrayList);
      string rowTemplate = CreateRowTemplate(maxColumnSizes);
      string rowSeparator = CreateRowSeperator(maxColumnSizes);

      resultSetStringBuilder.Append(rowSeparator);
      for (int i = 0; i < resultSetStringArrayList.Count; i++) {
        resultSetStringBuilder.AppendFormat(rowTemplate, (object[]) resultSetStringArrayList[i]).
            Append(rowSeparator);
      }
      return resultSetStringBuilder.ToString();
    }

    /// <summary>
    /// Creates the row template given the maximum size for each column.
    /// </summary>
    /// <param name="maxColumnSizes">The maximum size for each column</param>
    /// <returns>The row template to format row data into.</returns>
    private static string CreateRowTemplate(List<int> maxColumnSizes) {
      List<String> columnFormatSpecifiers = new List<string>();
      int i = 0;
      foreach (int maxColumnSize in maxColumnSizes) {
        columnFormatSpecifiers.Add(string.Format("{{{0},{1}}}", i, maxColumnSize));
        i++;
      }
      return new StringBuilder("| ").Append(string.Join(" | ", columnFormatSpecifiers.ToArray())).
          Append(" |\n").ToString();
    }

    /// <summary>
    /// Creates the row seperator given the maximum size for each column.
    /// </summary>
    /// <param name="maxColumnSizes"The maximum size for each column.></param>
    /// <returns>The row seperator.</returns>
    private static String CreateRowSeperator(List<int> maxColumnSizes) {
      StringBuilder rowSeperator = new StringBuilder("+");
      foreach (int maxColumnSize in maxColumnSizes) {
        for (int i = 0; i < maxColumnSize + 2; i++) {
          rowSeperator.Append("-");
        }
        rowSeperator.Append("+");
      }
      return rowSeperator.Append("\n").ToString();
    }

    /// <summary>
    /// Gets a list of the maximum size for each column.
    /// </summary>
    /// <param name="resultSet">The result set to process.</param>
    /// <returns>A list of the maximum size for each column.</returns>
    private static List<int> GetMaxColumnSizes(List<string[]> resultSet) {
      List<int> maxColumnSizes = new List<int>();
      for (int i = 0; i < resultSet[i].Length; i++) {
        int maxColumnSize = -1;
        for (int j = 0; j < resultSet.Count; j++) {
          if (resultSet[j][i].Length > maxColumnSize) {
            maxColumnSize = resultSet[j][i].Length;
          }
        }
        maxColumnSizes.Add(maxColumnSize);

      }
      return maxColumnSizes;
    }

    /// <summary>
    /// Gets the column labels for the result set.
    /// </summary>
    /// <param name="resultSet">The result set to get the column labels for.
    /// </param>
    /// <returns>The string array of column labels.</returns>
    public static String[] GetColumnLabels(ResultSet resultSet) {
      List<string> columnLabels = new List<string>();
      foreach (ColumnType column in resultSet.columnTypes) {
        columnLabels.Add(column.labelName);
      }
      return columnLabels.ToArray();
    }

    /// <summary>
    /// Gets the row values for a row of the result set in the form of an object
    /// array.
    /// </summary>
    /// <param name="row">The row to get the values for.</param>
    /// <returns>The object array of the row values.</returns>
    public static object[] GetRowValues(Row row) {
      List<object> rowValues = new List<object>();
      foreach (Value value in row.values) {
        rowValues.Add(GetValue(value));
      }
      return rowValues.ToArray();
    }

    /// <summary>
    /// Gets the row values for a row of the result set in a the form of a string
    /// array. <code>null</code> values are interperted as empty strings.
    /// </summary>
    /// <param name="row">The row to get the values for.</param>
    /// <returns>The string array of the row values.</returns>
    public static String[] GetRowStringValues(Row row) {
      object[] rowValues = GetRowValues(row);
      List<string> rowStringValues = new List<string>();
      foreach (object obj in rowValues) {
        rowStringValues.Add(getTextValue(obj));
      }
      return rowStringValues.ToArray();
    }

    /// <summary>
    /// Gets the text value of an object.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A formatted text representation of the value.</returns>
    /// <remarks>DateValue is formatted in yyyy-mm-dd format. DateTimeValue is
    /// formatted in yyyy-mm-dd HH:mm:ss Z format.</remarks>
    private static string getTextValue(Object value) {
      if (value == null) {
        return "";
      }

      if (value is DateValue) {
        Google.Api.Ads.Dfp.v201308.DateValue dateValue =
            (Google.Api.Ads.Dfp.v201308.DateValue) value;
        return string.Format("{0:0000}-{1:00}-{2:00}", dateValue.value.year, dateValue.value.month,
            dateValue.value.day);
      } else if (value is DateTimeValue) {
        Google.Api.Ads.Dfp.v201308.DateTimeValue dateTimeValue =
            (Google.Api.Ads.Dfp.v201308.DateTimeValue) value;
        return string.Format("{0:0000}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00} {6}",
            dateTimeValue.value.date.year, dateTimeValue.value.date.month,
            dateTimeValue.value.date.day, dateTimeValue.value.hour,
            dateTimeValue.value.minute, dateTimeValue.value.second,
            dateTimeValue.value.timeZoneID);
      } else {
        // NumberValue, BooleanValue, TextValue
        return value.ToString();
      }
    }
  }
}
