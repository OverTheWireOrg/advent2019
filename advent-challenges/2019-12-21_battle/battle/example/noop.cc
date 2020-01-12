#include <cmath>
#include <iostream>
#include <map>
#include <vector>

using namespace std;

int main() {
  srand(time(0));
  string dummy;
  cin >> dummy;
  for (int i = 0; i < 90; i++) {
    int x, y;
    cin >> x >> y;
  }

  while (true) {
    while (true) {
      if (!cin)
        return 0;
      string cmd;
      cin >> cmd;
      if (cmd == "star") {
        int id, richness, owner, shipcount, turns;
        cin >> id >> richness >> owner >> shipcount >> turns;
      } else if (cmd == "link") {
        int from, to;
        cin >> from >> to;
      } else if (cmd == "flight") {
        int from, to, owner, shipcount, turns;
        cin >> from >> to >> shipcount >> owner >> turns;
      } else if (cmd == "cpu") {
        int millis_left;
        cin >> millis_left;
      } else if (cmd == "done") {
        break;
      }
    }
    cout << "done" << endl;
  }
}
