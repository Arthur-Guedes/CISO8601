using Java.Util;

namespace Fr.Turri.JISO8601
{
	public class Iso8601Deserializer {
		private Iso8601Deserializer(){}

		public static Date ToDate(string toParse){
			return ToCalendar(toParse).Time;
		}

		public static Calendar ToCalendar(string toParse){
			int indexOfT = toParse.IndexOf('T');
			if ( indexOfT == -1 ){
				return BuildCalendarWithDateOnly(toParse, toParse);
			}
			Calendar result = BuildCalendarWithDateOnly(toParse.Substring(0, indexOfT), toParse);
			return ParseHour(result, toParse.Substring(indexOfT+1));
		}

		private static Calendar ParseHour(Calendar result, string hourStr){
			string basicFormatHour = hourStr.Replace(":", "");

			int indexOfZ = basicFormatHour.IndexOf('Z');
			if ( indexOfZ != -1 ){
				ParseHourWithoutHandlingTimeZone(result, basicFormatHour.Substring(0, indexOfZ));
			} else {
				int indexOfSign = GetIndexOfSign(basicFormatHour);
				if ( indexOfSign == -1 ){
					ParseHourWithoutHandlingTimeZone(result, basicFormatHour);
					result.TimeZone = TimeZone.Default;
				} else {
					ParseHourWithoutHandlingTimeZone(result, basicFormatHour.Substring(0, indexOfSign));
					result.TimeZone = TimeZone.GetTimeZone("GMT" + basicFormatHour.Substring(indexOfSign));
				}
			}
			return result;
		}

		private static int GetIndexOfSign(string str){
			int index = str.IndexOf('+');
			return index != -1 ? index : str.IndexOf('-');
		}

		private static void ParseHourWithoutHandlingTimeZone(Calendar calendar, string basicFormatHour){
			basicFormatHour = basicFormatHour.Replace(',', '.');
			int indexOfDot = basicFormatHour.IndexOf('.');
			double fractionalPart = 0;
			if ( indexOfDot != -1 ){
				fractionalPart = double.Parse("0" + basicFormatHour.Substring(indexOfDot));
				basicFormatHour = basicFormatHour.Substring(0, indexOfDot);
			}

			if ( basicFormatHour.Length >= 2 ){
				calendar.Set(CalendarField.HourOfDay, int.Parse(basicFormatHour.Substring(0, 2)));
			}

			if ( basicFormatHour.Length > 2 ){
				calendar.Set(CalendarField.Minute, int.Parse(basicFormatHour.Substring(2, 4)));
			} else {
				fractionalPart *= 60;
			}

			if ( basicFormatHour.Length > 4 ){
				calendar.Set(CalendarField.Second, int.Parse(basicFormatHour.Substring(4, 6)));
			} else {
				fractionalPart *= 60;
			}

			calendar.Set(CalendarField.Millisecond, (int) (fractionalPart * 1000));
		}

		private static Calendar BuildCalendarWithDateOnly(string dateStr, string originalDate){
			Calendar result = new GregorianCalendar(TimeZone.GetTimeZone("UTC"));
			result.MinimalDaysInFirstWeek = 4;
			result.FirstDayOfWeek = Calendar.Monday;
			result.Set(CalendarField.HourOfDay, 0);
			result.Set(CalendarField.Minute, 0);
			result.Set(CalendarField.Second, 0);
			result.Set(CalendarField.Millisecond, 0);
			string basicFormatDate = dateStr.Replace("-", "");

			if ( basicFormatDate.IndexOf('W') != -1 ){
				return ParseWeekDate(result, basicFormatDate);
			} else if ( basicFormatDate.Length == 7 ){
				return ParseOrdinalDate(result, basicFormatDate);
			} else {
				return ParseCalendarDate(result, basicFormatDate, originalDate);
			}
		}

		private static Calendar ParseCalendarDate(Calendar result, string basicFormatDate, string originalDate){
			if ( basicFormatDate.Length == 2 ){
				return ParseCalendarDateWithCenturyOnly(result, basicFormatDate);
			} else if ( basicFormatDate.Length == 4){
				return ParseCalendarDateWithYearOnly(result, basicFormatDate);
			} else {
				return ParseCalendarDateWithPrecisionGreaterThanYear(result, basicFormatDate, originalDate);
			}
		}

		private static Calendar ParseCalendarDateWithCenturyOnly(Calendar result, string basicFormatDate){
			result.Set(int.Parse(basicFormatDate) * 100, 0, 1);
			return result;
		}

		private static Calendar ParseCalendarDateWithYearOnly(Calendar result, string basicFormatDate){
			result.Set(int.Parse(basicFormatDate), 0, 1);
			return result;
		}

		private static Calendar ParseCalendarDateWithPrecisionGreaterThanYear(Calendar result, string basicFormatDate, string originalDate){
			int year = int.Parse(basicFormatDate.Substring(0, 4));
			int month = int.Parse(basicFormatDate.Substring(4, 6)) - 1;
			if ( basicFormatDate.Length == 6 ){
				result.Set(year, month, 1);
				return result;
			}

			if ( basicFormatDate.Length == 8 ){
				result.Set(year, month, int.Parse(basicFormatDate.Substring(6)));
				return result;
			}
			throw new Java.Lang.RuntimeException("Can't parse " + originalDate);
		}

		private static Calendar ParseWeekDate(Calendar result, string basicFormatDate) {
			result.Set(CalendarField.Year, int.Parse(basicFormatDate.Substring(0, 4)));
			result.Set(CalendarField.WeekOfYear, int.Parse(basicFormatDate.Substring(5, 7)));
			result.Set(CalendarField.DayOfWeek, basicFormatDate.Length == 7
					? Calendar.Monday
					: Calendar.Sunday + int.Parse(basicFormatDate.Substring(7)));
			return result;
		}

		private static Calendar ParseOrdinalDate(Calendar calendar, string basicFormatOrdinalDate) {
			calendar.Set(CalendarField.Year, int.Parse(basicFormatOrdinalDate.Substring(0, 4)));
			calendar.Set(CalendarField.DayOfYear, int.Parse(basicFormatOrdinalDate.Substring(4)));
			return calendar;
		}
	}
}
