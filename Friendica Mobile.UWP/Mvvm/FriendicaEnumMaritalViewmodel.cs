using Friendica_Mobile.UWP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Friendica_Mobile.UWP.Mvvm
{
    public class FriendicaEnumMaritalViewmodel : FriendicaEnumBaseViewmodel
    {
        public enum FriendicaMaritalValues
        {
            // id, english, german, spanish, italian, portuguese, french
            [DisplayString("NotSpecified", "Not specified", "Keine Angabe", "No especificado", "Imprecisato", "Indeterminado", "Indéterminé")]
            NotSpecified,
            [DisplayString("Single", "Single", "Single", "Soltero", "Single", "Solteiro(a)", "Célibataire")]
            Single,
            [DisplayString("Lonely", "Lonely", "Einsam", "Solitario", "Solitario", "Solitário(a)", "Esseulé")]
            Lonely,
            [DisplayString("Available", "Available", "Verfügbar", "Disponible", "Disponibile", "Disponível", "Disponible")]
            Available,
            [DisplayString("Unavailable", "Unavailable", "Nicht verfügbar", "No disponible", "Non disponibile", "Não disponível", "Indisponible")]
            Unavailable,
            [DisplayString("HasCrush", "Has crush", "verknallt", "Enamorado", "è cotto/a", "Tem uma paixão", "Attiré par quelqu'un")]
            HasCrush,
            [DisplayString("Infatuated", "Infatuated", "verliebt", "Loco/a por alguien", "infatuato/a", "Apaixonado", "Entiché")]
            Infatuated,
            [DisplayString("Dating", "Dating", "Dating", "De citas", "Disponibile a un incontro", "Saindo com alguém", "Dans une relation")]
            Dating,
            [DisplayString("Unfaithful", "Unfaithful", "Untreu", "Infiel", "Infedele", "Infiel", "Infidèle")]
            Unfaithful,
            [DisplayString("SexAddict", "Sex Addict", "Sexbesessen", "Adicto al sexo", "Sesso-dipendente", "Viciado(a) em sexo", "Accro au sexe")]
            SexAddict,
            [DisplayString("Friends", "Friends", "Kontakte", "Amigos", "Amici", "Amigos", "Amis")]
            Friends,
            [DisplayString("FriendsBenefits", "Friends/Benefits", "Freunde/Zuwendungen", "Amigos con beneficios", "Amici con benefici", "Amigos/Benefícios", "Amis par intérêt")]
            FriendsBenefits,
            [DisplayString("Casual", "Casual", "Casual", "Casual", "Casual", "Casual", "Casual")]
            Casual,
            [DisplayString("Engaged", "Engaged", "Verlobt", "Comprometido/a", "Impegnato", "Envolvido(a)", "Fiancé")]
            Engaged,
            [DisplayString("Married", "Married", "Verheiratet", "Casado/a", "Sposato", "Casado(a)", "Marié")]
            Married,
            [DisplayString("ImaginarilyMarried", "Imaginarily married", "imaginär verheiratet", "Casado imaginario", "immaginariamente sposato/a", "Casado imaginariamente", "Se croit marié")]
            ImaginarilyMarried,
            [DisplayString("Partners", "Partners", "Partner", "Socios", "Partners", "Parceiros", "Partenaire")]
            Partners,
            [DisplayString("Cohabiting", "Cohabiting", "zusammenlebend", "Cohabitando", "Coinquilino", "Coabitando", "En cohabitation")]
            Cohabiting,
            [DisplayString("CommonLaw", "Common law", "wilde Ehe", "Pareja de hecho", "diritto comune", "Direito comum", "Marié \"de fait\"/\"sui juris\" (concubin)")]
            CommonLaw,
            [DisplayString("Happy", "Happy", "Glücklich", "Feliz", "Felice", "Feliz", "Heureux")]
            Happy,
            [DisplayString("NotLooking", "Not looking", "Nicht auf der Suche", "No busca relación", "Non guarda", "Não estou procurando", "Pas intéressé")]
            NotLooking,
            [DisplayString("Swinger", "Swinger", "Swinger", "Swinger", "Scambista", "Swinger", "Échangiste")]
            Swinger,
            [DisplayString("Betrayed", "Betrayed", "Betrogen", "Traicionado/a", "Tradito", "Traído(a)", "Trahi€")]
            Betrayed,
            [DisplayString("Separated", "Separated", "Getrennt", "Separado/a", "Separato", "Separado(a)", "Séparé")]
            Separated,
            [DisplayString("Unstable", "Unstable", "Unstabil", "Inestable", "Instabile", "Instável", "Instable")]
            Unstable,
            [DisplayString("Divorced", "Divorced", "Geschieden", "Divorciado/a", "Divorziato", "Divorciado(a)", "Divorcé")]
            Divorced,
            [DisplayString("ImaginarilyDivorced", "Imaginarily divorced", "imaginär geschieden", "Divorciado imaginario", "immaginariamente divorziato/a", "Divorciado imaginariamente", "Se croit divorcé")]
            ImaginarilyDivorced,
            [DisplayString("Widowed", "Widowed", "Verwitwet", "Viudo/a", "Vedovo", "Viúvo(a)", "Veuf/Veuve")]
            Widowed,
            [DisplayString("Uncertain", "Uncertain", "Unsicher", "Incierto", "Incerto", "Incerto(a)", "Incertain")]
            Uncertain,
            [DisplayString("Complicated", "It's complicated", "Ist kompliziert", "Es complicado", "E' complicato", "É complicado", "C'est compliqué")]
            Complicated,
            [DisplayString("DontCare", "Don't care", "Ist mir nicht wichtig", "No te importa", "Non interessa", "Não importa", "S'en désintéresse")]
            DontCare,
            [DisplayString("AskMe", "Ask me", "Frag mich", "Pregúntame", "Chiedimelo", "Pergunte-me", "Me demander")]
            AskMe,
            [DisplayString("NotInDatabase", "Not in database", "Unbekannter Eintrag in Datenbank", "Entrada desconocida", "Entrata sconosciuta", "Entrada desconhecida", "Entrée inconnue")]
            NotInDatabase
        }


        public FriendicaEnumMaritalViewmodel()
        {
            Type = typeof(FriendicaMaritalValues);
            EmptyType = FriendicaMaritalValues.NotSpecified.ToString();
            UnknownType = FriendicaMaritalValues.NotInDatabase.ToString();
            PrepareDictionaries();           
        }
    }
}
