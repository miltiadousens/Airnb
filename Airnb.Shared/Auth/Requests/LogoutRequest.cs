using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace Airnb.Shared.Auth.Requests
{
    public record LogoutRequest([Required] string RefreshToken);
    /*Den ligner RefreshRequest til forveksling, og man kunne genbruge den.
    Men en request beskriver hvad man vil, ikke bare hvilke felter den har.
    Når man om et år læser Logout(RefreshRequest request), 
    ser det mærkeligt ud.
    Hvis logout senere skal have flere felter (fx “log ud på alle enheder”), 
    kan den vokse uden at påvirke refresh.*/
}
