using System;
using System.Text.RegularExpressions;

namespace IOPointManager.Models
{
    public class InstrumentTag
    {
        public const int MaxAreaLength = 6;
        public const int MinTagNumberLength = 1;
        public const int MaxTagNumberLength = 3;
        public const int MaxDeviceClassLength = 5;
        public const string TagPattern = "^[A-Z0-9-]+$";

        private static readonly char[] ValidSeparators = { '-', '_', '/', '.', ':', ';', '|' };

        public string? Area { get; set; }
        public string? DeviceClass { get; set; }
        public string? Loop { get; set; }
        public string? TagNumber { get; set; }
        public string? Suffix { get; set; }
        public string? FullTag { get; set; }
        public char? Separator { get; set; }

        public static InstrumentTag Parse(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new ArgumentException("Тег не может быть пустым или null");

            tag = tag.Trim();
            var result = new InstrumentTag { FullTag = tag };

            char? separator = null;
            int firstSepIndex = -1;
            foreach (char sep in ValidSeparators)
            {
                int idx = tag.IndexOf(sep);
                if (idx > 0 && (firstSepIndex == -1 || idx < firstSepIndex))
                {
                    firstSepIndex = idx;
                    separator = sep;
                }
            }

            if (separator == null)
            {
                ParseWithoutSeparator(tag, result);
                return result;
            }

            result.Separator = separator;
            string[] parts = tag.Split(separator.Value);

            if (tag[0] == separator.Value)
                throw new ArgumentException("Разделитель не может быть в начале строки");

            string firstPart = parts[0];
            if (firstPart.Length > 0 && Regex.IsMatch(firstPart, "^[0-9]+$"))
            {
                if (firstPart.Length > MaxAreaLength)
                    throw new ArgumentException($"Area не может быть длиннее {MaxAreaLength} символов");
                result.Area = firstPart;
                var newParts = new string[parts.Length - 1];
                Array.Copy(parts, 1, newParts, 0, parts.Length - 1);
                parts = newParts;
            }

            if (parts.Length < 2)
                throw new ArgumentException("Недостаточно компонентов тега");

            string devicePart = parts[0];
            if (!Regex.IsMatch(devicePart, "^[A-Za-z]+$"))
                throw new ArgumentException("DeviceClass должен содержать только буквы");
            if (devicePart.Length > MaxDeviceClassLength)
                throw new ArgumentException($"DeviceClass не может быть длиннее {MaxDeviceClassLength} символов");
            result.DeviceClass = devicePart.ToUpperInvariant();

            string remaining = parts.Length > 1 ? string.Join(separator.Value.ToString(), parts, 1, parts.Length - 1) : "";

            var loopMatch = Regex.Match(remaining, "^[0-9]+");
            if (!loopMatch.Success)
                throw new ArgumentException("Loop должен содержать цифры");
            string loopPart = loopMatch.Value;
            if (loopPart.Length > 5)
                throw new ArgumentException("Loop не может быть длиннее 5 символов");
            result.Loop = loopPart;

            string afterLoop = remaining.Substring(loopPart.Length);
            var tagMatch = Regex.Match(afterLoop, "^[0-9]+");
            if (tagMatch.Success)
            {
                string tagNum = tagMatch.Value;
                if (tagNum.Length > MaxTagNumberLength)
                    throw new ArgumentException($"TagNumber не может быть длиннее {MaxTagNumberLength} символов");
                result.TagNumber = tagNum;
                afterLoop = afterLoop.Substring(tagNum.Length);
            }

            if (!string.IsNullOrEmpty(afterLoop))
            {
                if (afterLoop.Length > 3)
                    throw new ArgumentException("Suffix не может быть длиннее 3 символов");
                result.Suffix = afterLoop.ToUpperInvariant();
            }

            return result;
        }

        private static void ParseWithoutSeparator(string tag, InstrumentTag result)
        {
            var deviceMatch = Regex.Match(tag, "^[0-9]*([A-Za-z]+)");
            if (!deviceMatch.Success || deviceMatch.Groups[1].Length == 0)
                throw new ArgumentException("Не удалось определить DeviceClass");

            int deviceStart = deviceMatch.Groups[1].Index;
            result.DeviceClass = deviceMatch.Groups[1].Value.ToUpperInvariant();
            if (result.DeviceClass.Length > MaxDeviceClassLength)
                throw new ArgumentException($"DeviceClass не может быть длиннее {MaxDeviceClassLength} символов");

            if (deviceStart > 0)
            {
                string area = tag.Substring(0, deviceStart);
                if (area.Length > MaxAreaLength)
                    throw new ArgumentException($"Area не может быть длиннее {MaxAreaLength} символов");
                result.Area = area;
            }

            string remaining = tag.Substring(deviceStart + result.DeviceClass.Length);

            var loopMatch = Regex.Match(remaining, "^[0-9]+");
            if (!loopMatch.Success)
                throw new ArgumentException("Loop должен содержать цифры");
            result.Loop = loopMatch.Value;

            string afterLoop = remaining.Substring(loopMatch.Value.Length);

            var tagMatch = Regex.Match(afterLoop, "^[0-9]+");
            if (tagMatch.Success)
            {
                if (tagMatch.Value.Length > MaxTagNumberLength)
                    throw new ArgumentException($"TagNumber не может быть длиннее {MaxTagNumberLength} символов");
                result.TagNumber = tagMatch.Value;
                afterLoop = afterLoop.Substring(tagMatch.Value.Length);
            }

            if (!string.IsNullOrEmpty(afterLoop))
            {
                if (afterLoop.Length > 3)
                    throw new ArgumentException("Suffix не может быть длиннее 3 символов");
                result.Suffix = afterLoop.ToUpperInvariant();
            }

            result.Separator = null;
        }

        public override string ToString() => FullTag ?? string.Empty;
    }
}