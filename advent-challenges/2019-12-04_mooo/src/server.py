#!/usr/bin/env python3
from flask import Flask, render_template, request, url_for, redirect, session, escape
import subprocess
import tempfile
import re

app = Flask(__name__)
cow_names = ["default", "custom", "apt", "bud-frogs", "bunny", "calvin", "cheese", "cock", "cower", "daemon",  "dragon", "dragon-and-cow", "duck", "elephant", "elephant-in-snake", "eyes", "flaming-sheep", "ghostbusters", "gnu", "hellokitty", "kiss", "koala", "kosh", "luke-koala", "mech-and-cow", "milk", "moofasa", "moose", "pony", "pony-smaller", "ren", "sheep", "skeleton", "snowman", "stegosaurus", "stimpy", "suse", "three-eyes", "turkey", "turtle", "tux", "unipony", "unipony-smaller", "vader", "vader-koala", "www"]

custom_cow  = "        $thoughts   ^__^\n"
custom_cow += "         $thoughts  ($eyes)\\\\_______\n"
custom_cow += "            (__)\\\\       )\\\\/\\\\\n"
custom_cow += "             $tongue ||----w |\n"
custom_cow += "                ||     ||"

app.secret_key = 'khhb\x0bDYt|)>o2\x0c-4; LS),XLLEQ|2Q\rc7i9+e~MCr\nf`TP<">ar<XUTGC#XD}\\Sm' 

@app.route('/', methods=['GET'])
def index():
	return render_template("index",cow_names=cow_names)

@app.route('/', methods=['POST'])
def say():
	m = request.form.get("message")
	cow = request.form.get("cow")
	if cow not in cow_names:
		return render_template("index",cow_names=cow_names,error="No such cow!")
	if cow == "custom":
		return redirect(url_for("cow_designer", message=m))
	output = cowsay(m,cow)
	if output == None:
		return render_template("index",cow_names=cow_names,error="cowsay failed!")
	return render_template("index",cow_names=cow_names,cow=escape(output))

@app.route('/cow_designer', methods=['GET'])
def cow_designer():
	if not session.get("custom_cow"):
		session["custom_cow"] = custom_cow.rstrip()
		session["eyes"] = "oo"
		session["tongue"] = "  "
	return render_template("cow_designer",custom_cow=session.get("custom_cow"),
		eyes=session.get("eyes"),tongue=session.get("tongue"), message=escape(request.args.get("message","Mooo!")))

@app.route('/cow_designer', methods=['POST'])
def say_custom():
	m = request.form.get("message","Mooo!")
	custom_cow =request.form.get("custom_cow").rstrip()
	tongue = request.form.get("tongue")
	eyes = request.form.get("eyes")
	session["custom_cow"] = custom_cow
	session["eyes"] = eyes
	session["tongue"] = tongue

	filter_cow = custom_cow[:]
	filter_cow = filter_cow.replace("$thoughts"," ")
	filter_cow = filter_cow.replace("$tongue"," ")
	filter_cow = filter_cow.replace("$eyes"," ")
	filter_cow = filter_cow.replace("\\\\"," ")

	error1 = "$ must be escaped with \\ except when using $thoughts, $eyes or $tongue"
	dollars = [d.start() for d in re.finditer(r"\$",filter_cow)]
	for d in dollars:
		if d == 0 or filter_cow[d-1] != '\\':
			return render_template("cow_designer",error=error1,custom_cow=escape(session.get("custom_cow")),
				eyes=escape(session.get("eyes")),tongue=escape(session.get("tongue")), message=escape(m))

	error2 = "@, {, }, [ and ] must be escaped with \\"
	ats = [d.start() for d in re.finditer(r"[\{\}@\[\]]",filter_cow)]
	for d in ats:
		if d == 0 or filter_cow[d-1] != '\\':
			return render_template("cow_designer",error=error2,custom_cow=escape(session.get("custom_cow")),
				eyes=escape(session.get("eyes")),tongue=escape(session.get("tongue")), message=escape(m))

	cow = '$the_cow = <<"EOC";\n' + custom_cow + "\nEOC\n"
	with tempfile.NamedTemporaryFile(suffix=".cow") as cowfile:
		cowfile.write(cow.encode("utf-8"))
		cowfile.flush()
		cow = custom_cowsay(m,cowfile.name,eyes,tongue)
		if cow == None:
			return render_template("cow_designer",error="Cowsay failed!",custom_cow=escape(session.get("custom_cow")),
				eyes=escape(session.get("eyes")),tongue=escape(session.get("tongue")), message=escape(m))
		return render_template("cow_designer",cow=escape(cow),custom_cow=escape(session.get("custom_cow")),
		eyes=escape(session.get("eyes")),tongue=escape(session.get("tongue")), message=escape(m))
	return render_template("cow_designer",error="Error creating cowfile!",custom_cow=escape(session.get("custom_cow")),
		eyes=escape(session.get("eyes")),tongue=escape(session.get("tongue")), message=escape(m))

def cowsay(message, cow):
	try:
		output = subprocess.check_output(['/usr/games/cowsay', "-f", cow, "--", message],timeout=5)
	except:
		return None
	return output.decode("utf-8")

def custom_cowsay(message, cowfile, eyes, tongue):
	try:
		output = subprocess.check_output(['/usr/games/cowsay', "-f", cowfile, "-T", tongue, "-e", eyes, "--", message],timeout=5)
	except:
		return None
	return output.decode("utf-8")

if __name__ == '__main__':
    app.run()