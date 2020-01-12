#include <iostream>
#include <sys/resource.h>
#include <sys/time.h>

using namespace std;

int main() {
  // Disable synchronization to make cin/cout much faster.
  // Don't use scanf/printf then. Of course, you can do your
  // own scanf without using cin/cout if you decide to.
  // This is just an example.
  std::ios::sync_with_stdio(false);

  // Read initial galaxy configuration.
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
      } else if (cmd == "done") {
        break;
      }
    }

    // Call getrusage to query how much CPU time you have left.
    // Be a bit on the conservative side, since the moment the process exceeds
    // this limit, it is killed and your AI does nothing for the rest of the
    // match.
    struct rusage usage;
    getrusage(RUSAGE_SELF, &usage);
    int usec_remaining =
        2000000 - ((usage.ru_utime.tv_sec + usage.ru_stime.tv_sec) * 1000000 +
                   usage.ru_utime.tv_usec + usage.ru_stime.tv_usec);

    // Issue some random flights just for demonstration.
    for (int i = 0; i < 2; i++) {
      for (int j = 3; j < 5; j++) {
        cout << "fly " << i << " " << j << " 1" << endl;
      }
    }
    cout << "done" << endl;
  }
}
