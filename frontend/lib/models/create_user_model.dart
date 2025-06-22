import 'package:json_annotation/json_annotation.dart';

part 'create_user_model.g.dart';

@JsonSerializable()
class CreateUserModel {
  final String? email;
  final String? userName;
  final String? password;

  CreateUserModel({this.email, this.userName, this.password});

  factory CreateUserModel.fromJson(Map<String, dynamic> json) => _$CreateUserModelFromJson(json);
  Map<String, dynamic> toJson() => _$CreateUserModelToJson(this);
}
