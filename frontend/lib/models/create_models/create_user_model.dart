import 'package:flutter/widgets.dart';
import 'package:json_annotation/json_annotation.dart';

part 'create_user_model.g.dart';

@JsonSerializable()
@immutable
class CreateUserModel {
  final String email;
  final String userName;
  final String password;

  const CreateUserModel({
    required this.email,
    required this.userName,
    required this.password,
  });

  factory CreateUserModel.fromJson(Map<String, dynamic> json) =>
      _$CreateUserModelFromJson(json);
  Map<String, dynamic> toJson() => _$CreateUserModelToJson(this);
}
