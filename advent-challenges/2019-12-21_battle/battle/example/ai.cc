#include <cmath>
#include <iostream>
#include <map>
#include <vector>

using namespace std;

int main() {
  srand(time(0));
  vector<pair<int, int>> star_pos;
  string dummy;
  cin >> dummy;
  for (int i = 0; i < 90; i++) {
    int x, y;
    cin >> x >> y;
    star_pos.push_back({x, y});
  }

  auto dist = [&](int a, int b) {
    int xdiff = star_pos[a].first - star_pos[b].first;
    int ydiff = star_pos[a].second - star_pos[b].second;
    return sqrt(xdiff * xdiff + ydiff * ydiff);
  };

  while (true) {
    while (true) {
      if (!cin)
        return 0;
      string cmd;
      cin >> cmd;
      if (cmd == "star") {
        int id, richness, owner, shipcount, turns;
        cin >> id >> richness >> owner >> shipcount >> turns;
        if (owner == 0) {
          std::vector<int> eligible_dests;
          for (int other = 0; other < 90; other++) {
            if (other == id)
              continue;
            if (dist(id, other) <= 60 && shipcount > 30) {
              eligible_dests.push_back(other);
            }
          } 
          if (eligible_dests.size() > 0) {
            int other = eligible_dests[rand() % eligible_dests.size()];
            cout << "fly " << id << " " << other << " " << shipcount / 2
                  << endl;
          }
        }
      } else if (cmd == "link") {
        int from, to;
        cin >> from >> to;
      } else if (cmd == "flight") {
        int from, to, owner, shipcount, turns;
        cin >> from >> to >> shipcount >> owner >> turns;
      } else if (cmd == "done") {
        break;
      }
    }
    cout << "done" << endl;
  }
}
