#include <iostream>
#include <vector>
#include <tbb/parallel_for.h>
#include <tbb/task_group.h>
#include <tbb/parallel_reduce.h>

class MyOp {
const int *my_tab;
public:
int a;
void operator () (const tbb::blocked_range<size_t> &r)
{
    const int *tab = my_tab;
    int ta = a;
    for (size_t i = r.begin(); i != r.end(); ++i) {
        std::cout << i << std:: endl;
        ta =+ tab[i]*tab[i]; //F(ta, tab[i]); --> reduction function
    }
    std::cout << std::endl;
    a = ta;
}
MyOp (MyOp &x, tbb::split): my_tab (x.my_tab), a(0) {} // mytab = x.my_tab, a = 0
void join (const MyOp &y) { a += y.a;} // a = F(a,y.a)
MyOp (const int tab[]): my_tab(tab), a(0) {} // my_tab = tab, a = 0.
};

int main () {
    int tab[10];
    int i;
    for (i = 0; i < 10; i++) tab[i] = i; // Data initialization
    MyOp op(tab); // the object 'op' is created with 'tab' as the argument
    tbb::parallel_reduce(tbb::blocked_range<size_t> (0,10), op);
    std::cout << op.a << "\n";
    return 0;
}