using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Session
{
    public bool master_access = false;
    public bool login = false;
    public int timecount = 0;
    public string db_id_account = "";

    public bool change_pass = false;
    public string code_pass = "";
}