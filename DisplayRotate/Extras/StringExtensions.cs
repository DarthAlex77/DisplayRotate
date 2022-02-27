/* 
 * This file is part of the AutoDisplayRotateApp distribution (https://github.com/DarthAlex77/DisplayRotate).
 * Copyright (c) 2022 Aleksey Surgaev.
 * 
 * This program is free software: you can redistribute it and/or modify  
 * it under the terms of the GNU General Public License as published by  
 * the Free Software Foundation, version 3.
 *
 * This program is distributed in the hope that it will be useful, but 
 * WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
 * General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License 
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System.Text.RegularExpressions;

namespace DisplayRotate.Extras
{
    public static class StringExtensions
    {
        public static bool? ToBoolean(this string value)
        {
            string newString = Regex.Replace(value, "[^.0-9]", "");
            return newString.ToLower().Trim() switch
            {
                "1" => true,
                "0" => false,
                _   => null
            };
        }
    }
}