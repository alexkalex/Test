using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProgramCentrvd
{
    class Program
    {
        static void Main(string[] args)
        {
            string localPath = AppDomain.CurrentDomain.BaseDirectory;

            DataTable dtDep = ConvertCSVtoDataTable(localPath + @"\department.csv");
            DataTable dtEmpl = ConvertCSVtoDataTable(localPath + @"\employee.csv");

            var data = from employee in dtEmpl.AsEnumerable()
                       join department in dtDep.AsEnumerable() on (string)employee["department_id"] equals (string)department["Id"]
                       select new
                       {
                           Id = (string)employee["id"],
                           department_id = (string)employee["department_id"],
                           department_name = (string)department["Name"],
                           chief_id = (string)employee["chief_id"],
                           name = (string)employee["name"],
                           salary = Convert.ToInt32(employee["salary"])
                       };


            // Суммарная зарплата в разрезе департаментов без руководителей
            var sumDepSalary = data.GroupBy(row => row.department_name).Select(g =>
                                new
                                {
                                    department_name = g.Key,
                                    salary = g.Sum(r => Convert.ToInt32(r.salary)),
                                    salaryMan = data.Where(m => m.Id == data.Where(c => c.department_name == g.Key).First().chief_id).First().salary
                                });

            foreach (var item in sumDepSalary)
            {
                Console.WriteLine(String.Format("Департамент = {0}, Суммарная зарплата без руководителя  = {1}, Суммарная зарплата с руководителем = {2}", item.department_name, item.salary, item.salary + item.salaryMan));
            }
            Console.WriteLine();

            //Департамент, в котором у сотрудника зарплата максимальна
            var maxSalary = data.OrderByDescending(x => x.salary).First();
            Console.WriteLine(String.Format("В департаменте: {0}, у сотрудника {1} максимальная зарплата = {2}", maxSalary.department_name, maxSalary.name, maxSalary.salary));
            Console.WriteLine();

            //Зарплаты руководителей департаментов (по убыванию)
            var salaryMan = data.Where(m => data.Select(x => x.chief_id).Distinct().Where(d => d != "NULL").Contains(m.Id)).OrderByDescending(o => o.salary);
            foreach (var item in salaryMan)
            {
                Console.WriteLine(String.Format("У руководителя {0}, зарплата = {1}", item.name, item.salary));
            }
            
            Console.ReadKey();
        }
        public static DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(strFilePath))
            {
                string[] headers = sr.ReadLine().Split(';');
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }
                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(';');
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        dr[i] = rows[i];
                    }
                    dt.Rows.Add(dr);
                }

            }

            return dt;
        }
    }
}
