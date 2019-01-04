using Friendica_Mobile.UWP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Friendica_Mobile.UWP.Mvvm
{
    public class FriendicaEnumGenderViewmodel : FriendicaEnumBaseViewmodel
    {
        public enum FriendicaGenderValues
        {
            // id, englisch, german, spanish, italian, portuguese, french
            [DisplayString("NotSpecified", "Not specified", "Keine Angabe", "No especificado", "Imprecisato", "Indeterminado", "Indéterminé")]
            NotSpecified,
            [DisplayString("Male", "Male", "Männlich", "Hombre", "Maschio", "Masculino", "Masculin")]
            Male,
            [DisplayString("Female", "Female", "Weiblich", "Mujer", "Femmina", "Feminino", "Féminin")]
            Female,
            [DisplayString("CurrentlyMale", "Currently Male", "Momentan männlich", "Actualmente Hombre", "Al momento maschio", "Atualmente masculino", "Actuellement masculin")]
            CurrentlyMale,
            [DisplayString("CurrentlyFemale", "Currently Female", "Momentan weiblich", "Actualmente Mujer", "Al momento femmina", "Atualmente feminino", "Actuellement féminin")]
            CurrentlyFemale,
            [DisplayString("MostlyMale", "Mostly Male", "Hauptsächlich männlich", "Mayormente Hombre", "Prevalentemente maschio", "Masculino a maior parte do tempo", "Principalement masculin")]
            MostlyMale,
            [DisplayString("MostlyFemale", "Mostly Female", "Hauptsächlich weiblich", "Mayormente Mujer", "Prevalentemente femmina", "Feminino a maior parte do tempo", "Principalement féminin")]
            MostlyFemale,
            [DisplayString("Transgender", "Transgender", "Transgender", "Transgenérico", "Transgender", "Transgênero", "Transgenre")]
            Transgender,
            [DisplayString("Intersex", "Intersex", "Intersex", "Bisexual", "Intersex", "Intersexual", "Inter-sexe")]
            Intersex,
            [DisplayString("Transsexual", "Transsexual", "Transsexuell", "Transexual", "Transessuale", "Transexual", "Transsexuel")]
            Transsexual,
            [DisplayString("Hermaphrodite", "Hermaphrodite", "Hermaphrodit", "Hermafrodita", "Ermafrodito", "Hermafrodita", "Hermaphrodite")]
            Hermaphrodite,
            [DisplayString("Neuter", "Neuter", "Neuter", "Neutro", "Neutro", "Neutro", "Neutre")]
            Neuter,
            [DisplayString("NonSpecific", "Non-specific", "Nicht spezifiziert", "Sin especificar", "Non specificato", "Não específico", "Non-spécifique")]
            NonSpecific,
            [DisplayString("Other", "Other", "Andere", "Otro", "Altro", "Outro", "Autre")]
            Other,
            [DisplayString("Undecided", "Undecided", "Unentschieden", "Indeciso", "Indeciso", "Indeciso", "Indécis")]
            Undecided,
            [DisplayString("NotInDatabase", "Not in database", "Unbekannter Eintrag in Datenbank", "Entrada desconocida", "Entrata sconosciuta", "Entrada desconhecida", "Entrée inconnue")]
            NotInDatabase
        }

        public FriendicaEnumGenderViewmodel()
        {
            Type = typeof(FriendicaGenderValues);
            EmptyType = FriendicaGenderValues.NotSpecified.ToString();
            UnknownType = FriendicaGenderValues.NotInDatabase.ToString();
            PrepareDictionaries();           
        }
    }
}
