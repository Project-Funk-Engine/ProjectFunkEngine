class_name SavekeeperNode extends Node

const Delimiter : String = "|"
const SaveFileDirectory : String = "user://saved_games";
const SaveFileExtension : String = ".save";
const DefaultSaveFileName : String = "game";

signal saving
signal game_saved
signal loaded
	
var GameSaveObjects : Dictionary[String, String]

func handle_prev_save() -> void:
	GameSaveObjects.clear()

func sanitize_savestring(what : String) -> String:
	if what.is_empty() or what == null: return ""
	what = what.strip_edges()
	if what.is_empty():
		return ""
		
	if what.find(Delimiter) == 0:
		return what.trim_prefix(Delimiter)
	return ""

func record_save(save_path : String = DefaultSaveFileName + SaveFileExtension) -> bool:
	saving.emit()
	return save_to_file(save_path)
	
	
func save_to_file(save_path : String = DefaultSaveFileName + SaveFileExtension) -> bool:
	if save_path.is_empty() or save_path == null:
		return false
	var file : FileAccess = FileAccess.open(SaveFileDirectory + "/" + save_path, FileAccess.WRITE)
	if file == null:
		return false
		
	for key in GameSaveObjects:
		file.store_line(key + Delimiter + GameSaveObjects[key])
	file.close()
	
	game_saved.emit()
	return true

func load_from_file(save_path : String = DefaultSaveFileName + SaveFileExtension) -> bool:
	if save_path.is_empty() or save_path == null:
		return false
	var file : FileAccess = FileAccess.open(SaveFileDirectory + "/" + save_path, FileAccess.READ)
	if file == null:
		return false
		
	handle_prev_save()
	while file.get_position() < file.get_length():
		var line : String = file.get_line()
		var idx : int = line.find(Delimiter)
		if idx == -1:
			return false
		var key : String = line.substr(0, idx)
		var value : String = sanitize_savestring(line.substr(idx))
		if value.is_empty():
			continue
		GameSaveObjects[key] = value
		
	loaded.emit()
	return true
	
const InvalidFormatString : String = "InvalidString"
	
func format(val_name : String, what : Variant) -> String:
	if what == null:
		return ""
	if typeof(what) == typeof(Delimiter) && !what.is_valid_filename():#TODO: regex to remove characters?
		return val_name + Delimiter + InvalidFormatString + Delimiter
	return val_name + Delimiter + str(what) + Delimiter
	
const ArrayDelimiter : String = "*"

func format_array(val_name : String, what : Array) -> String:
	if what == null:
		return ""
	var return_string : String = val_name + Delimiter + "["
	for obj in what:
		if typeof(obj) == typeof(Delimiter) && !obj.is_valid_filename():#TODO: regex to remove characters?
			return_string = return_string + InvalidFormatString + ArrayDelimiter
			continue
		return_string = return_string + str(obj) + ArrayDelimiter
	return_string = return_string + "]" + Delimiter
	return return_string
	
class Parse_Result:
	var value : Variant
	var success : bool
	var next_idx : int
	var message : String
	
	func create(val : Variant, was_success : bool, next : int, msg : String = "Success") -> Parse_Result:
		value = val
		success = was_success
		next_idx = next
		message = msg
		return self
		
func parse_to_value_string(val_name : String, save_string : String, start_idx : int) -> Parse_Result:
	var next_idx : int = save_string.find(val_name, start_idx)
	if next_idx == -1:
		return Parse_Result.new().create(null, false, start_idx, "Name not found! %s" % val_name)
	next_idx += val_name.length() + 1
	var final_idx : int = save_string.find(Delimiter, next_idx)
	if final_idx == -1:
		return Parse_Result.new().create(null, false, start_idx, "No final delimiter found around value! String received: %s, from position %s" % [save_string, next_idx])
	var value : String = save_string.substr(next_idx, final_idx - next_idx)
	return Parse_Result.new().create(value, true, final_idx)
	
#validator and converter are string methodnames. for type string use is_valid_filename and strip_escapes
func parse(val_name : String, save_string : String, start_idx : int, validator : String, converter : String) -> Parse_Result:
	if val_name.is_empty() or save_string.is_empty():
		return Parse_Result.new().create(null, false, start_idx, "String was empty! %s %s" % [val_name, save_string])
	
	var success : Parse_Result = parse_to_value_string(val_name, save_string, start_idx)
	if !success.success:
		return success
	var value : String = success.value
	var final_idx : int = success.next_idx
	
	if !Callable.create(value, validator).call():
		return Parse_Result.new().create(null, false, start_idx, "Unable to parse %s to given type!" % value)
		
	var final = Callable.create(value, converter).call()
	return Parse_Result.new().create(final, true, final_idx)

func parse_array(val_name : String, save_string : String, start_idx : int, validator : String, converter : String) -> Parse_Result:
	if val_name.is_empty() or save_string.is_empty():
		return Parse_Result.new().create(null, false, start_idx, "String was empty! %s %s" % [val_name, save_string])
	
	var success : Parse_Result = parse_to_value_string(val_name, save_string, start_idx)
	if !success.success:
			return success
	var value : String = success.value
	var final_idx : int = success.next_idx
	
	value = value.trim_prefix("[").trim_suffix("]")
	var values : PackedStringArray = value.split(ArrayDelimiter, false)
	var finals : Array[Variant] = []
	for s in values:
		if !Callable.create(s, validator).call():
			return Parse_Result.new().create(finals, false, start_idx, "Unable to parse %s to given type! Returning any successful values!" % s)
		finals.push_back(Callable.create(s, converter).call())
		
	return Parse_Result.new().create(finals, true, final_idx)

	
	
	
		
