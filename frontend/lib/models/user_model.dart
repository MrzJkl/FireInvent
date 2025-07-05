import 'package:flutter/widgets.dart';
import 'package:json_annotation/json_annotation.dart';
import 'create_models/create_user_model.dart';

part 'user_model.g.dart';

@JsonSerializable()
@immutable
class UserModel extends CreateUserModel {
  final String id;

  const UserModel({
    required this.id,
    required super.email,
    required super.userName,
    required super.password,
  });

  factory UserModel.fromJson(Map<String, dynamic> json) =>
      _$UserModelFromJson(json);
  @override
  Map<String, dynamic> toJson() => _$UserModelToJson(this);
}
