using System;
using System.Collections.Generic;
using System.Linq;

namespace FishermansSail.BoatRigs
{
    internal static class FishermansStaysailDefinitions
    {
        // Installed mast ancestry, authored rather than selected by proximity.
        internal static int Base(string boat, int section) => Sections(boat, section).Last();

        internal static int[] Sections(string boat, int section)
        {
            var sections = new List<int>();
            while (section >= 0)
            {
                sections.Add(section);
                section = Parent(boat, section);
            }
            return sections.ToArray();
        }

        private static int Parent(string boat, int section)
        {
            switch (boat)
            {
                case "BOAT medi medium (50)":
                    switch (section)
                    {
                        case 3:
                            return -1;
                        case 2:
                            return -1;
                        case 67:
                            return -1;
                        case 79:
                            return -1;
                        case 5:
                            return -1;
                        case 4:
                            return -1;
                        case 7:
                            return -1;
                        case 6:
                            return -1;
                        case 53:
                            return -1;
                        case 54:
                            return -1;
                        case 55:
                            return 3;
                        case 57:
                            return 2;
                        case 56:
                            return 5;
                        case 58:
                            return 4;
                        case 59:
                            return 7;
                        case 60:
                            return 6;
                    }
                    break;
                case "BOAT junk medium (80)":
                    switch (section)
                    {
                        case 8:
                            return -1;
                        case 9:
                            return -1;
                        case 58:
                            return -1;
                        case 10:
                            return -1;
                        case 11:
                            return -1;
                        case 12:
                            return -1;
                        case 13:
                            return -1;
                        case 57:
                            return -1;
                        case 53:
                            return 12;
                        case 62:
                            return 9;
                    }
                    break;
                case "BOAT junk large (70)":
                    switch (section)
                    {
                        case 0:
                            return -1;
                        case 1:
                            return -1;
                        case 67:
                            return -1;
                        case 2:
                            return -1;
                        case 51:
                            return -1;
                        case 3:
                            return -1;
                        case 52:
                            return -1;
                        case 4:
                            return -1;
                        case 53:
                            return -1;
                    }
                    break;
                case "BOAT dhow medium (20)":
                    switch (section)
                    {
                        case 10:
                            return -1;
                        case 11:
                            return -1;
                        case 12:
                            return -1;
                        case 55:
                            return -1;
                        case 69:
                            return -1;
                        case 13:
                            return 10;
                        case 14:
                            return 11;
                        case 51:
                            return -1;
                        case 62:
                            return -1;
                        case 64:
                            return 51;
                        case 59:
                            return 55;
                        case 70:
                            return 69;
                        case 80:
                            return 12;
                    }
                    break;
                case "BOAT medi small (40)":
                    switch (section)
                    {
                        case 6:
                            return -1;
                        case 5:
                            return -1;
                        case 8:
                            return -1;
                        case 57:
                            return -1;
                        case 56:
                            return -1;
                        case 68:
                            return -1;
                    }
                    break;
                case "BOAT LEOPARD (207)":
                    switch (section)
                    {
                        case 4:
                            return -1;
                        case 5:
                            return 4;
                        case 6:
                            return 5;
                        case 7:
                            return -1;
                        case 8:
                            return 7;
                        case 9:
                            return 8;
                        case 10:
                            return -1;
                        case 11:
                            return 10;
                        case 12:
                            return 11;
                    }
                    break;
                case "BOAT Shroud Large":
                    switch (section)
                    {
                        case 6:
                            return -1;
                        case 5:
                            return -1;
                        case 8:
                            return -1;
                        case 7:
                            return -1;
                        case 10:
                            return -1;
                        case 9:
                            return -1;
                    }
                    break;
            }
            throw new ArgumentException("No authored staysail mast reference.");
        }
    }
}
