# DÚ6B: Deque 2.2 Cool, I love C#

**Při řešení této úlohy je povinnost použít C# koncept enumerátorových metod pro implementaci libovolného enumerátoru = je vyžadováno použití `yield return`!**

Naimplementujte datovou strukturu typicky pojmenovávanou Deque, neboli double-
ended queue, čili obousměrná fronta. Základní princip je podobný jako u
obyčejné FIFO fronty, ale Deque umožňuje hodnoty přidávat na oba konce, i z
obou konců odebírat. Důležité je, že všechny 4 operace (přidání prvku na
jeden/druhý konec a odebrání prvku z jednoho/druhého konce) musí být
implementace schopna zvládnout v amortizovaně konstantním čase. Kromě
implementace uvedené datové struktury také navrhněte nové rozhraní, které bude
definovat kontrakt pro všechny možné implementace Deque (a vaše implementace
by samozřejmě toto rozhraní měla také implementovat). Toto rozhraní by mělo
vhodně doplňovat již stávající sadu rozhraní .NET pro kolekce.

Jako interní datovou strukturu ve vašem řešení **povinně** použijte
implementaci pomocí pole polí, tak jak je typicky implementována třída `deque`
v STL knihovně C++:

![Příklad struktury deque](https://recodex.mff.cuni.cz:4000/v1/uploaded-files/e52aed76-aba7-11e7-a937-00505601122b/download)

[_Původní zdroj: <http://kremer.cpsc.ucalgary.ca/STL/1024x768/deque.html> (M.
Nelson. C++ Programmer’s Guide to the Standard Template Library. IDG Books
Worldwide, Inc. Foster City, CA, 1995)_]  
kde velikost primárního pole `Map` se zvětšuje dle potřeby, velikost každého
`Data Block`u je kontantní.

Odevzdaná implementace by měla být generická s jedním typovým parametrem
určujícím typ položek v Deque uložených. Kromě výše uvedeného obecného chování
by odevzdaná implementace měla implementovat též standardní rozhraní
`IList<T>` a poskytovat přístup k libovolné exitující položce v konstantním
čase, tj. O(1).

Dále očekávejte, že častým příkladem použití bude procházení prvků ve frontě z
obou směrů a že také často budeme chtít měnit způsob chápání toho, kde je
začátek a kde konec fronty – tj. navrhněte také doplňkovou datovou strukturu,
která umožní inverzní pohled na frontu (tj. např. přidání prvku na začátek
fronty je ekvivalentní přidání prvku na konec inverzního pohledu na tutéž
frontu; nebo pokud budeme foreach cyklem procházet všechny prvky fronty, tak
je získáme v opačném pořadí, než kdybychom foreach cyklem procházeli její
inverzní pohled).

Cílem této úlohy je, abyste si vyzkoušeli návrh knihovny (a v ní
implementované datové struktury) tak, aby odpovídala pravidlům a konvencím
běžným v prostředí .NET. Proto také zdrojový soubor odevzdaný do CodExu nebude
hotovým programem. V této úloze nebudete tedy číst žádný vstup, ani žádný
výstup vypisovat a vaše implementace bude pouze podrobena sadě unit testů.
Abychom nemuseli implementovat komplikovaný unit test framework, tak po vaší
implementaci požadujeme:

  * nesmí obsahovat metodu `Main` v žádné třídě
  * musí obsahovat veřejnou generickou třídu pojmenovanou `Deque` a s jedním typovým parametrem bez omezení
  * musí obsahovat veřejnou statickou testovací třídu `DequeTest`, která testům CodExu poskytne inverzní pohled z existující instance vaší `Deque` (váš `Deque` ale navrhněte tak, aby její inverzní pohled bylo možno získat i bez použití třídy `DequeTest`)

Třída `DequeTest` musí být naimplementována podle následujícího prototypu:

    
    
    public static class DequeTest {
    	public static IList<T> GetReverseView<T>(Deque<T> d) {
    		...
    	}
    }

Pro snazší ladění případných chyb v odevzdaných řešeních následuje seznam
všech testů, kde je pro každý test uveden výčet metod, které daný test na
odevzdané implementaci Deque testuje:

    
    
    
    1: foreach, Add
    2: [], Count, Add
    3: Add, Insert(0, …), foreach, [], Count
    4: Remove, Add, Insert(0, …), foreach, [], Count
    5: Insert, Remove, Add, foreach, [], Count
    6: IndexOf, RemoveAt, Insert, Add, foreach, [], Count
    7: IndexOf, Remove, Insert, Add, foreach, [], Count
    8: IndexOf, Remove, Insert, Add, foreach, [], Count (velký vstup)
    9: IndexOf, Remove, Insert, Add, foreach, [], Count (velký vstup)
    10: IndexOf, Remove, Insert, Add, foreach, [], Count (velký vstup)
    11: IndexOf, Remove, Insert, Add, foreach, [], Count (velký vstup)
    12: IndexOf, Remove, Insert, Add, foreach, [], Count (velký vstup)
    13: IndexOf, Remove, Insert, Add, foreach, [], Count (velký vstup, chybné parametry, indexy mimo meze, neexistující prvky)
    14: IndexOf, Remove, Insert, Add, foreach, [], Count (velký vstup, chybné parametry, indexy mimo meze, neexistující prvky)
    15: foreach, [], Count na ReverseView, Add, Insert(0, …), foreach, [], Count
    16: foreach, [], Count na ReverseView, IndexOf, Remove, Insert, Add, foreach, [], Count (velký vstup, chybné parametry, indexy mimo meze, neexistující prvky)
    17: foreach, [], Count na ReverseView, IndexOf, Remove, Insert, Add, foreach, [], Count (velký vstup, chybné parametry, indexy mimo meze, neexistující prvky)
    18: IndexOf, Insert, Add, foreach, [], Count
    19: IndexOf, foreach, [], Count na ReverseView, Clear, IndexOf, Insert, Add, foreach, [], Count
    20: Clear, Insert, Remove, Add, foreach, [], Count (velký vstup)
    21: RemoveAt, Add, foreach, [], Count (velký vstup)
    22: RemoveAt(0), RemoveAt, Add, foreach, [], Count (velký vstup)
    23: Insert(0, …), RemoveAt(0), RemoveAt, Add, foreach, [], Count (velký vstup)
    24: CopyTo, Insert, RemoveAt, Add, foreach, [], Count (velký vstup)
    25: Insert, RemoveAt, Add, foreach, [], Count na Deque i na její ReverseView (velký vstup)
    26: Insert(0, …), RemoveAt(0), Add, [], Count (velmi velký vstup)
    27: Add, RemoveAt, [], Count (velmi velký vstup)
    28: modifikace Deque během enumerace → ocekávána InvalidOperationException
    29: modifikace Deque během enumerace → ocekávána InvalidOperationException
    30: modifikace Deque během enumerace → ocekávána InvalidOperationException
    31: modifikace Deque během enumerace → ocekávána InvalidOperationException
    32: modifikace Deque během enumerace → ocekávána InvalidOperationException
    33: modifikace Deque během enumerace → ocekávána InvalidOperationException