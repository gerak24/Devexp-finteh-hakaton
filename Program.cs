using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles();

app.MapGet("/", context =>
{
    context.Response.Redirect("/index.html");
    return Task.CompletedTask;
});


app.MapPost("/getdata", async (test test) => ParseCalculate(test.start, test.end, test.name));
app.MapPost("/analyze1", async (test test)=>
{
    return Calculate1(ParseCalculate(test.start, test.end, test.name));
    
});
app.MapPost("/analyze2", async (test test)=>
{
    return Calculate2(ParseCalculate(test.start, test.end, test.name));
});
app.MapPost("/analyze3", async (test test)=>
{
    return Calculate3(ParseCalculate(test.start, test.end, test.name));
});


List<stat> ParseCalculate(string firstDate, string secondData, string name)
{
    var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture)
    {
        Delimiter = ",",
    };
    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        PrepareHeaderForMatch = args => args.Header.ToLower(),
    };
    
    List<stat> statistic = new List<stat>();

    
    using (var reader = new StreamReader("wwwroot/fh_5yrs.csv"))
    using (var csv = new CsvReader(reader, config))
    {
        var records = csv.GetRecords<InvestDate>();
        DateTime start = DateTime.ParseExact(firstDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        DateTime end = DateTime.ParseExact(secondData, "yyyy-MM-dd", CultureInfo.InvariantCulture);


        foreach (var investDate in records)
        {
            try
            {
                investDate.data = investDate.data.Replace("-", ".");
                string symbol = investDate.symbol;
                var dataTime = DateTime.Parse(investDate.data);
                string adjclose = (investDate.adjclose);
                if (dataTime > start && dataTime < end && symbol == name)
                {
                    statistic.Add(new stat(dataTime, adjclose, symbol));
                }
            }
            catch
            {
                // ignored
            }
        }
    }

    return statistic;
}

string Calculate1(List<stat> parseCalculate)
{
    int chkdrop = 0, bufdrop = 0, chkup = 0, bufup = 0;
    int i = 0;
    double money = 10000;
    int kolvo = 100;
    money -= kolvo * double.Parse(parseCalculate[0].adjcloser, CultureInfo.InvariantCulture);
    var output = new ResultOfStrategy();
    output = dataset1(parseCalculate[0].dateTimer, chkdrop, bufdrop, chkup, bufup);
    //money = output.moneyResult;

    ResultOfStrategy dataset1(DateTime dateTime, int chkdrop, int bufdrop, int chkup, int bufup)
    {

        foreach (var lines in parseCalculate)
        {
            i++;
            var resultOfDay = new ResulOfDay();
            resultOfDay.day = i;
            resultOfDay.kolvo = kolvo;

            output.ResulOfDays.Add(resultOfDay);

            if (kolvo > 0)
            {
                bufdrop = dropcheck(chkdrop, bufdrop, lines.dateTimer,
                    double.Parse(lines.adjcloser, CultureInfo.InvariantCulture), i).bufdrop;
                chkup = upcheck(chkup, bufup, lines.dateTimer,
                    double.Parse(lines.adjcloser, CultureInfo.InvariantCulture), i).chkup;
                bufup = upcheck(chkup, bufup, lines.dateTimer,
                    double.Parse(lines.adjcloser, CultureInfo.InvariantCulture), i).bufup;
                if (bufdrop == 2 && chkup == 1)
                {
                    if (money >= (double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture)))
                    {
                        money = buy(money, double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture),
                            lines.dateTimer);

                        if (bufup == 3 && (double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture) *
                                           1.02) < double.Parse(parseCalculate[i - 1].adjcloser, CultureInfo.InvariantCulture))
                        {
                            money = sell(money, double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture),
                                lines.dateTimer);
                        }
                    }
                }

                if (double.Parse(parseCalculate[0].adjcloser, CultureInfo.InvariantCulture) >
                    1.10 * double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture))
                {
                    money += kolvo * double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture);
                    kolvo = 0;
                }

                if (i == 11)
                {
                    money += kolvo * double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture);
                    kolvo = 0;
                }

                output.moneyResult = (money-10000).ToString();
            }
        }

        return output;
    }

    double buy(double money, double adjclose, DateTime dateTime)
    {
        kolvo += 1;
        money -= adjclose;
        var dayIngo = new ResulOfDay
        {
            day = i,
            kolvo = kolvo,
            sellOrBuy = true
        };
        output.ResulOfDays.Add(dayIngo);
        return money;
    }

    double sell(double money, double adjclose, DateTime dateTime)
    {
        money += adjclose;
        kolvo -= 1;
        var dayIngo = new ResulOfDay
        {
            day = i,
            kolvo = kolvo,
            sellOrBuy = false
        };
        output.ResulOfDays.Add(dayIngo);
        return money;
    }

    (int bufdrop, int chkdrop) dropcheck(int chkdrop, int bufdrop, DateTime dateTime, double adjclose, int i)
    {
        if (double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture) <
            double.Parse(parseCalculate[i - 1].adjcloser, CultureInfo.InvariantCulture))
        {
            chkdrop++;
        }
        else
        {
            bufdrop = chkdrop;
            chkdrop = 0;
        }

        return (bufdrop, chkdrop);
    }

    (int bufup, int chkup) upcheck(int chkup, int bufup, DateTime dateTime, double adjclose, int i)
    {
        if (double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture) >
            double.Parse(parseCalculate[i - 1].adjcloser, CultureInfo.InvariantCulture))
        {
            chkup++;
        }
        else
        {
            bufup = chkup;
            chkup = 0;
        }

        return (chkup, bufup);
    }
    return output.moneyResult;
}

string Calculate2(List<stat> parseCalculate)
{
    int chkdrop = 0, bufdrop = 0, chkup = 0, bufup = 0;
    int i = 0;
    double money = 10000;
    int kolvo = 100;
    money -= kolvo * double.Parse(parseCalculate[0].adjcloser, CultureInfo.InvariantCulture);
    var output = new ResultOfStrategy();
    output = dataset2(parseCalculate[0].dateTimer, chkdrop, bufdrop, chkup, bufup);
    //money = output.moneyResult;

    ResultOfStrategy dataset2(DateTime dateTime, int chkdrop, int bufdrop, int chkup, int bufup)
    {
        foreach (var lines in parseCalculate)
        {
            i++;
            var resultOfDay = new ResulOfDay();
            resultOfDay.day = i;
            resultOfDay.kolvo = kolvo;

            output.ResulOfDays.Add(resultOfDay);

            if (kolvo > 0)
            {
                if (bufdrop == 3 && chkup == 1)
                {
                    if (money >= (double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture)))
                    {
                        money = buy(money, double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture),
                            lines.dateTimer);
                    }
                }

                if (bufup == 4 && chkdrop == 1)
                {
                    money = sell(money, double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture),
                        lines.dateTimer);
                }

                if (double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture) * 1.15 < double.Parse(parseCalculate[0].adjcloser, CultureInfo.InvariantCulture))
                {
                    money += kolvo * double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture);
                    kolvo = 0;
                }
                if (chkdrop==10)
                {
                    money += kolvo * double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture);
                    kolvo = 0;
                }
                if(i==29){ 
                    money += kolvo * double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture);
                    kolvo = 0;
                    
                }
                output.moneyResult = (money-10000).ToString();
            }
                
            return output;
        }

        output.moneyResult = (money - 10000).ToString();

        return output;
        double buy(double money, double adjclose, DateTime dateTime)
        {
            kolvo += 1;
            money -= adjclose;
            var dayIngo = new ResulOfDay
            {
                day = i,
                kolvo = kolvo,
                sellOrBuy = true
            };
            output.ResulOfDays.Add(dayIngo);
            return money;
        }

        double sell(double money, double adjclose, DateTime dateTime)
        {
            money += adjclose;
            kolvo -= 1;
            var dayIngo = new ResulOfDay
            {
                day = i,
                kolvo = kolvo,
                sellOrBuy = false
            };
            output.ResulOfDays.Add(dayIngo);
            return money;
        }

        (int bufdrop, int chkdrop) dropcheck(int chkdrop, int bufdrop, DateTime dateTime, double adjclose, int i)
        {
            if (double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture) <
                double.Parse(parseCalculate[i - 1].adjcloser, CultureInfo.InvariantCulture))
            {
                chkdrop++;
            }
            else
            {
                bufdrop = chkdrop;
                chkdrop = 0;
            }

            return (bufdrop, chkdrop);
        }

        (int bufup, int chkup) upcheck(int chkup, int bufup, DateTime dateTime, double adjclose, int i)
        {
            if (double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture) >
                double.Parse(parseCalculate[i - 1].adjcloser, CultureInfo.InvariantCulture))
            {
                chkup++;
            }
            else
            {
                bufup = chkup;
                chkup = 0;
            }

            return (chkup, bufup);
        }
        
    }
    
    double buy(double money, double adjclose, DateTime dateTime)
    {
        kolvo += 1;
        money -= adjclose;
        var dayIngo = new ResulOfDay
        {
            day = i,
            kolvo = kolvo,
            sellOrBuy = true
        };
        output.ResulOfDays.Add(dayIngo);
        return money;
    }

    double sell(double money, double adjclose, DateTime dateTime)
    {
        money += adjclose;
        kolvo -= 1;
        var dayIngo = new ResulOfDay
        {
            day = i,
            kolvo = kolvo,
            sellOrBuy = false
        };
        output.ResulOfDays.Add(dayIngo);
        return money;
    }

    (int bufdrop, int chkdrop) dropcheck(int chkdrop, int bufdrop, DateTime dateTime, double adjclose, int i)
    {
        if (double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture) <
            double.Parse(parseCalculate[i - 1].adjcloser, CultureInfo.InvariantCulture))
        {
            chkdrop++;
        }
        else
        {
            bufdrop = chkdrop;
            chkdrop = 0;
        }

        return (bufdrop, chkdrop);
    }

    (int bufup, int chkup) upcheck(int chkup, int bufup, DateTime dateTime, double adjclose, int i)
    {
        if (double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture) >
            double.Parse(parseCalculate[i - 1].adjcloser, CultureInfo.InvariantCulture))
        {
            chkup++;
        }
        else
        {
            bufup = chkup;
            chkup = 0;
        }

        return (chkup, bufup);
    }
    return output.moneyResult;
}
string Calculate3(List<stat> parseCalculate)
{
    int chkdrop = 0, bufdrop = 0, chkup = 0, bufup = 0;
    int i = 0;
    double money = 10000;
    int kolvo = 100;
    money -= kolvo * double.Parse(parseCalculate[0].adjcloser, CultureInfo.InvariantCulture);
    var output = new ResultOfStrategy();
    output = dataset3(parseCalculate[0].dateTimer, chkdrop, bufdrop, chkup, bufup);
    //money = output.moneyResult;

    ResultOfStrategy dataset3(DateTime dateTime, int chkdrop, int bufdrop, int chkup, int bufup)
    {
        foreach (var lines in parseCalculate)
        {
            i++;
            var resultOfDay = new ResulOfDay();
            resultOfDay.day = i;
            resultOfDay.kolvo = kolvo;

            output.ResulOfDays.Add(resultOfDay);

            if (kolvo > 0)
            {
                if (bufdrop == 2)
                {
                    if (money >= (double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture)))
                    {
                        money = buy(money, double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture),
                            lines.dateTimer);
                    }

                    if (bufup == 4)
                    {
                        money = sell(money, double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture),
                            lines.dateTimer);
                    }
                }
                if(i > 5){
                    if (double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture) * 1.05 <
                        double.Parse(parseCalculate[i - 5].adjcloser, CultureInfo.InvariantCulture))
                    {
                        money += kolvo * double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture);
                        kolvo = 0;
                    }
                }

                if (i > 10)
                {
                    if (double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture) * 1.15 >
                        double.Parse(parseCalculate[i - 10].adjcloser, CultureInfo.InvariantCulture))
                    {
                        money += kolvo * double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture);
                        kolvo = 0;
                    }
                }

                if(i==19)
                { 
                    money += kolvo * double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture);
                    kolvo = 0;
                    
                }
                output.moneyResult = (money-10000).ToString();
            }

            return output;
        }

    double buy(double money, double adjclose, DateTime dateTime)
    {
        kolvo += 1;
        money -= adjclose;
        var dayIngo = new ResulOfDay
        {
            day = i,
            kolvo = kolvo,
            sellOrBuy = true
        };
        output.ResulOfDays.Add(dayIngo);
        return money;
    }

    double sell(double money, double adjclose, DateTime dateTime)
    {
        money += adjclose;
        kolvo -= 1;
        var dayIngo = new ResulOfDay
        {
            day = i,
            kolvo = kolvo,
            sellOrBuy = false
        };
        output.ResulOfDays.Add(dayIngo);
        return money;
    }

    (int bufdrop, int chkdrop) dropcheck(int chkdrop, int bufdrop, DateTime dateTime, double adjclose, int i)
    {
        if (double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture) <
            double.Parse(parseCalculate[i - 1].adjcloser, CultureInfo.InvariantCulture))
        {
            chkdrop++;
        }
        else
        {
            bufdrop = chkdrop;
            chkdrop = 0;
        }

        return (bufdrop, chkdrop);
    }

    (int bufup, int chkup) upcheck(int chkup, int bufup, DateTime dateTime, double adjclose, int i)
    {
        if (double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture) >
            double.Parse(parseCalculate[i - 1].adjcloser, CultureInfo.InvariantCulture))
        {
            chkup++;
        }
        else
        {
            bufup = chkup;
            chkup = 0;
        }

        return (chkup, bufup);
    }
    return output ;
}
    double buy(double money, double adjclose, DateTime dateTime)
    {
        kolvo += 1;
        money -= adjclose;
        var dayIngo = new ResulOfDay
        {
            day = i,
            kolvo = kolvo,
            sellOrBuy = true
        };
        output.ResulOfDays.Add(dayIngo);
        return money;
    }

    double sell(double money, double adjclose, DateTime dateTime)
    {
        money += adjclose;
        kolvo -= 1;
        var dayIngo = new ResulOfDay
        {
            day = i,
            kolvo = kolvo,
            sellOrBuy = false
        };
        output.ResulOfDays.Add(dayIngo);
        return money;
    }

    (int bufdrop, int chkdrop) dropcheck(int chkdrop, int bufdrop, DateTime dateTime, double adjclose, int i)
    {
        if (double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture) <
            double.Parse(parseCalculate[i - 1].adjcloser, CultureInfo.InvariantCulture))
        {
            chkdrop++;
        }
        else
        {
            bufdrop = chkdrop;
            chkdrop = 0;
        }

        return (bufdrop, chkdrop);
    }

    (int bufup, int chkup) upcheck(int chkup, int bufup, DateTime dateTime, double adjclose, int i)
    {
        if (double.Parse(parseCalculate[i].adjcloser, CultureInfo.InvariantCulture) >
            double.Parse(parseCalculate[i - 1].adjcloser, CultureInfo.InvariantCulture))
        {
            chkup++;
        }
        else
        {
            bufup = chkup;
            chkup = 0;
        }

        return (chkup, bufup);
    }

    return output.moneyResult ;
}




app.Run();


class ResultOfStrategy
{
    //public string mem;
    public string moneyResult = "";
    public List<ResulOfDay> ResulOfDays = new List<ResulOfDay>();
}
class ResulOfDay
{
    public int day = 0;
    public int kolvo = 0;
    public bool sellOrBuy = false;
}


class InvestDate
{
    [Index(0)] public string data { get; set; }
    [Index(1)] public string volume { get; init; }
    [Index(2)] public string open { get; init; }
    [Index(3)] public string hight { get; init; }
    [Index(4)] public string low { get; init; }
    [Index(5)] public string close { get; init; }
    [Index(6)] public string adjclose { get; init; }
    [Index(7)] public string symbol { get; init; }

    public string dota;
}


public class stat
{
    public stat(DateTime dateTime, string adjclose, string symbol)
    {
        this.adjcloser = adjclose;
        this.dateTimer = dateTime;
        this.symbol = symbol;
    }

    public string adjcloser { get; set; } = "";
    public DateTime dateTimer { get; set; } = new DateTime();
    public string symbol { get; set; } = "";

    public string getStart { get; set; } = "";
    public string getEnd { get; set; } = "";
}

public class cash
{
    public double money = 0;

    public cash(double money)
    {
        this.money = money;
    }
}
class test
{
    public string start { get; set; } = "";
    public string end { get; set; } = "";
    public string name { get; set; } = "";

}