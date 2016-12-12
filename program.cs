//Программа, строящая дерево выражений по математическому выражение (опрделены круглые скобки, +,-,*,/ и десятичные дроби)
//
//из проблем:
//Программа очень тяжело расширяемая, особенно для введния унарных операций. Стоило абстрагировать разбиение выражения
//на подвыражения и понятие "операции"
//Не орабатывает выражение типа "-(2+3)" т.к., по сути, в этом выражении унарная операция, которая не определена.
//Однако обробатывает выражение типа "-2"(сделано при помощи костыля, который не срабатывает только в листьях)
//Лишнее количество проверок на "правильные скобки". Можно было сделать единственную проверку на правильность выражения,
//но я решил отказаться от этого в пользу того, чтобы какждый Node был независим. Т.е. не было разницы в том
//корневой это узел или он где-то в иерархии (эта идея мне очень нравится)
//По-моему, у класса получился неплохой интерфейс - Конструктор для создания объекта и метод Compute(). Стоило ещё оставить
//доступными для чтения детей, чтобы можно было просматривать всю иерархию.
//Можно было еще распаралелить создания левых и правых детей, тем более, что разделяемых данных у них нет.


using System;

namespace Calculator {

    public class ExpressionNode {
        public enum Operation {
            Addition,
            Substraction,
            Multiplication,
            Division,
            NotOperation
        }

        private Operation Operat;
        public float? Value { get; private set; } = null;

        private ExpressionNode Child1 = null;
        private ExpressionNode Child2 = null;

        private string stringExpression;

        public ExpressionNode(string stringExpression) {
            
            //Проверка на нормальные скобки
            if (!isRightBreackets(stringExpression))
                throw new FormatException("Неверное выражение для вычисления. Плохие скобки");
            this.stringExpression = stringExpression;
            string leftExpression, rightExpression;
            Operation operation;
            
            beatStringExpression(out leftExpression,out rightExpression, out operation);
            this.Operat = operation;
            //Если это выражение без операции, то получим его значение
            if (operation == Operation.NotOperation) {
                float res;
                if (!float.TryParse(leftExpression, out res)) {
                    throw new FormatException("Неверное выражение для вычисления. плохие числа");
                }
                Value = res;
            }
            else {
                Child1 = new ExpressionNode(leftExpression);
                Child2 = new ExpressionNode(rightExpression);
            }
        }

        //метод разбивает выражения на подвыражения
        private void beatStringExpression(out string leftExpression, out string rightExpression, 
            out Operation operation) {
            //Работаем с копией, чтобы сохранить исходную строку выражений            
            string bufStringExpression = stringExpression;
            //убираем пробелы
            bufStringExpression = bufStringExpression.Replace(" ", "");
            //заменяем точки запятыми
            bufStringExpression = bufStringExpression.Replace(".", ",");
            //Если строка пустая, значит не вставили число (несколько операторов подряд)
            if (bufStringExpression.Length == 0)
                throw new FormatException("Где-то пропустили число");
            //убираем внешние скобки
            while (bufStringExpression[0] == '(' && bufStringExpression[bufStringExpression.Length - 1] == ')') {
                //Если строка пустая, значит не вставили число (несколько операторов подряд)
                string bufStringExpression1 = bufStringExpression.Substring(1, bufStringExpression.Length - 2);
                if (isRightBreackets(bufStringExpression1))
                    bufStringExpression = bufStringExpression1;
                else
                    break;
                //Если строка пустая, значит не вставили число (несколько операторов подряд)
                if (bufStringExpression.Length == 0)
                    throw new FormatException("Где-то пропустили число");
            }

            //Значение по умолчанию. Вернутся, если не найдены знаки операций
            leftExpression = rightExpression = bufStringExpression;
            operation = Operation.NotOperation;

            //признак того, что идём внутри скобок
            int inBrackets = 0;
            //проходим по строке и ищем первый разделительный знак (+ или -) вне скобок
            for (int i = 0; i < bufStringExpression.Length; i++) {
                char c = bufStringExpression[i];
                //если скобки, то дальше
                if (c == '(') {
                    inBrackets++;
                    continue;
                } 
                if(c == ')') {
                    inBrackets--;
                    continue;
                }
                if ((c == '+' || c == '-') && (inBrackets == 0)) {
                    //Если это первый знак -, то это не оператор
                    if (i == 0 && (c == '-' || c == '+' ))
                        continue;
                    if (c == '-')
                        operation = Operation.Substraction;
                    else
                        operation = Operation.Addition;

                    leftExpression = bufStringExpression.Substring(0, i);
                    rightExpression = bufStringExpression.Substring(i + 1, bufStringExpression.Length - i - 1);
                    
                    return;
                }
            }

            //Если не нашли таких, то ищем (* или /)
			//Повторяющийся код, надо заменить
            for (int i = 0; i < bufStringExpression.Length; i++) {
                char c = bufStringExpression[i];
                //если скобки, то дальше
                if (c == '(') {
                    inBrackets++;
                    continue;
                }
                if (c == ')') {
                    inBrackets--;
                    continue;
                }
                if ((c == '*' || c == '/') && (inBrackets == 0)) {
                    leftExpression = bufStringExpression.Substring(0, i);
                    rightExpression = bufStringExpression.Substring(i + 1, bufStringExpression.Length - i - 1);
                    if (c == '*')
                        operation = Operation.Multiplication;
                    else
                        operation = Operation.Division;
                    return;
                }
            }

            //Если и таких не нашли, то это выражение не разбивается.
        }
        //метод вычисляет значение Value
        public void Compute() {
            if (this.Value != null)
                return;
			//Должна быть проверка на null у детей, но, по идее, такое может произойти только, если Value != null (условие выше)
            if (this.Child1.Value == null)
                this.Child1.Compute();
            if (this.Child2.Value == null)
                this.Child2.Compute();
            switch (Operat) {
                case Operation.Addition:
                    Value = Child1.Value + Child2.Value;
                    break;
                case Operation.Substraction:
                    Value = Child1.Value - Child2.Value;
                    break;
                case Operation.Multiplication:
                    Value = Child1.Value * Child2.Value;
                    break;
                case Operation.Division:
                    Value = Child1.Value / Child2.Value;
                    break;
            }

        }
        //Метод проверяет, нормальные ли скобки
        private static bool isRightBreackets(string str) {
            int breckets = 0;
            foreach (char c in str) {
                if (c == '(')
                    breckets++;
                else if (c == ')')
                    breckets--;
                if (breckets < 0)
                    return false;
            }
            if (breckets != 0)
                return false;
            return true;
        }
    }


    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Калькулятор.");
            
            //  
            string expressionString = "";
            while (expressionString != "1") {
                Console.WriteLine("Введите выражение (для выхода, введите \"1\"):");
                expressionString = Console.ReadLine();
                try {
                    ExpressionNode calc = new ExpressionNode(expressionString);
                    calc.Compute();
                    Console.WriteLine("\nРезультат: " + calc.Value);
                }
                catch(FormatException e) {
                    Console.WriteLine(e.Message);
                };                
            }

            //Console.ReadKey();
        }
    }
}
