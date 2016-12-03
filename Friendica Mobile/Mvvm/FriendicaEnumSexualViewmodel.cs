using Friendica_Mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Friendica_Mobile.Mvvm
{
    public class FriendicaEnumSexualViewmodel : FriendicaEnumBaseViewmodel
    {
        public enum FriendicaSexualValues
        {
            // id, englisch, german, spanish, italian, portuguese, french
            [DisplayString("NotSpecified", "Not specified", "Keine Angabe", "No especificado", "Imprecisato", "Indeterminado", "Indéterminé")]
            NotSpecified,
            [DisplayString("Males", "Males", "Männer", "Hombres", "Maschi", "Homens", "Hommes")]
            Males,
            [DisplayString("Females", "Females", "Frauen", "Mujeres", "Femmine", "Mulheres", "Femmes")]
            Females,
            [DisplayString("Gay", "Gay", "Schwul", "Gay", "Gay", "Gays", "Gay")]
            Gay,
            [DisplayString("Lesbian", "Lesbian", "Lesbisch", "Lesbiana", "Lesbica", "Lésbicas", "Lesbienne")]
            Lesbian,
            [DisplayString("NoPreference", "No preference", "Keine Vorlieben", "Sin preferencias", "Nessuna preferenza", "Sem preferência", "Sans préférence")]
            NoPreference,
            [DisplayString("Bisexual", "Bisexual", "Bisexuell", "Bisexual", "Bisessuale", "Bissexuais", "Bisexuel")]
            Bisexual,
            [DisplayString("Autosexual", "Autosexual", "Autosexuell", "Autosexual", "Autosessuale", "Autossexuais", "Auto-sexuel")]
            Autosexual,
            [DisplayString("Abstinent", "Abstinent", "Abstinent", "Célibe", "Astinente", "Abstêmios", "Abstinent")]
            Abstinent,
            [DisplayString("Virgin", "Virgin", "Jungfrauen", "Virgen", "Vergine", "Virgens", "Vierge")]
            Virgin,
            [DisplayString("Deviant", "Deviant", "Deviant", "Desviado", "Deviato", "Desviantes", "Déviant")]
            Deviant,
            [DisplayString("Fetish", "Fetish", "Fetisch", "Fetichista", "Fetish", "Fetiches", "Fétichiste")]
            Fetish,
            [DisplayString("Oodles", "Oodles", "Orgien", "Orgiástico", "Un sacco", "Insaciável", "Oodles")]
            Oodles,
            [DisplayString("Nonsexual", "Nonsexual", "Nonsexual", "Asexual", "Asessuato", "Não sexual", "Non-sexuel")]
            Nonsexual,
            [DisplayString("NotInDatabase", "Not in database", "Unbekannter Eintrag in Datenbank", "Entrada desconocida", "Entrata sconosciuta", "Entrada desconhecida", "Entrée inconnue")]
            NotInDatabase
        }

        public FriendicaEnumSexualViewmodel()
        {
            Type = typeof(FriendicaSexualValues);
            EmptyType = FriendicaSexualValues.NotSpecified.ToString();
            UnknownType = FriendicaSexualValues.NotInDatabase.ToString();
            PrepareDictionaries();           
        }
    }
}
