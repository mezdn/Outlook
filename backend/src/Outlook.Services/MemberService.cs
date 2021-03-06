﻿using Outlook.Models.Core.Dtos;
using Outlook.Models.Core.Models;
using Outlook.Models.Services;
using System.Collections.Generic;
using System.Linq;

namespace Outlook.Services
{
    public class MemberService
    {
        //public static Position[] ArabicPositions = { Position.المحرر, Position.رئيس_تحرير, Position.رئيس_قسم, Position.عضو_سابق, Position.كاتب_صحفي, Position.مدقق_الموقع, Position.مدقق_النسخة, Position.مدقق_لغوي, Position.نائب_المحرر };
        //public static Position[] EnglishPositions = { Position.Editor_In_Chief, Position.Senior_Editor, Position.Associate_Editor, Position.Junior_Editor, Position.Proofreader, Position.Copy_Editor, Position.Web_Editor, Position.Former_Member, Position.Staff_Writer };
        //public static Position[] NonBoardMembers = { Position.Staff_Writer, Position.Former_Member, Position.كاتب_صحفي, Position.عضو_سابق };

        /// <summary>
        /// IsJuniorEditor is a method that returns whether a member is an arabic or english junior editor
        /// </summary>
        /// <param name="member"></param>
        /// <returns>boolean result</returns>
        public bool IsJuniorEditor(Member member) => (member.Position == OutlookConstants.Position.Junior_Editor) || (member.Position == OutlookConstants.Position.رئيس_قسم);

        /// <summary>
        /// AddBoardMembers is a method that retrieves all the members that belong to a section, given its positions, from a given set of members
        /// </summary>
        /// <param name="section"></param>
        /// <param name="positions">positions of the mentioned sections</param>
        /// <param name="boardMembers">set of members</param>
        public static void AddBoardMembers(Dictionary<string, IEnumerable<MemberDto>> section, IOrderedEnumerable<OutlookConstants.Position> positions, IEnumerable<MemberDto> boardMembers)
        {
            foreach (var position in positions)
            {
                if (!OutlookConstants.NonBoardMembers.Contains(position))
                {
                    var members = from member in boardMembers
                                  where member.Position == position.ToString().Replace('_', ' ')
                                  select member;

                    section[position.ToString().Replace('_', ' ')] = members;
                }
            }
        }
    }
}
