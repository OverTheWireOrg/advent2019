package main

import (
	"io/ioutil"
	"log"
	"net/http"
	"os/exec"
	"strings"
	"syscall"
	"time"
)

func main() {

	if connected() {
		//log.Fatal("Has Internet!")
		log.Print("Has Internet! QUIT if not testing")
	}

	i := 0
	cmd := exec.Command("/opt/got_server/got_shell")
	cmd.SysProcAttr = &syscall.SysProcAttr{Setpgid: true}
	cmd.Start()

	time.Sleep(5 * time.Second)

	for true {
		if !runTimeout(5*time.Second, checkHealth) {
			i++
			log.Printf("Unresponsive... %d", i)
		} else {
			i = 0
		}
		if i >= 3 {
			log.Print("Stopping Process!")
			i = 0
			pgid, err := syscall.Getpgid(cmd.Process.Pid)
			if err == nil {
				syscall.Kill(-pgid, syscall.SIGKILL) // note the minus sign
			}
			cmd.Wait()
			log.Print("Restarting")
			cmd = exec.Command("/opt/got_server/got_shell")
			cmd.Start()
		}
		time.Sleep(5 * time.Second)
	}
}

func checkHealth() bool {
	client := http.Client{
		Timeout: 3 * time.Second,
	}
	//log.Print("Checking heartbeat")
	resp, err := client.Get("http://0.0.0.0:1224/?cmd=echo%20heartbeat")
	if err != nil {
		return false
	}
	body, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		return false
	}

	if strings.Contains(string(body), "heartbeat") {
		//log.Print("Healthy!")
		return true
	}

	log.Print("Not healthy!")
	log.Printf("%s", string(body))
	return false

}

func connected() (ok bool) {
	_, err := http.Get("http://clients3.google.com/generate_204")
	if err != nil {
		return false
	}
	return true
}

func runTimeout(timeout time.Duration, doFunc func() bool) bool {
	select {
	case <-time.After(timeout):
		break

	default:
		return doFunc()
	}

	return false
}
